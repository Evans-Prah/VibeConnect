using System.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using VibeConnect.Post.Module.DTOs.Comment;
using VibeConnect.Shared;
using VibeConnect.Shared.Extensions;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Post.Module.Services.Comment;

public class CommentService(IBaseRepository<Storage.Entities.Comment> commentRepository, IBaseRepository<Storage.Entities.Post> postRepository,
    IBaseRepository<User> userRepository, ILoggerAdapter<CommentService> logger,
    IBaseRepository<PostLike> postLikeRepository) : ICommentService
{
    public async Task<ApiResponse<CommentResponseDto>> AddComment(string? username, CommentRequestDto commentRequestDto)
    {
        try
        {
            logger.LogInformation(
                "User {user} is adding a comment to post {postId} -> Service: {service} -> Method: {method}. Payload {payload}",
                username,
                commentRequestDto.PostId,
                nameof(CommentService),
                nameof(AddComment),
                JsonConvert.SerializeObject(commentRequestDto)
            );

            var user = await userRepository.FindOneAsync(x => x.Username == username);

            if (user == null)
            {
                return new ApiResponse<CommentResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Sorry! User does not exist, check and try again."
                };
            }

            var post = await postRepository.GetByIdAsync(commentRequestDto.PostId);

            if (post == null)
            {
                return new ApiResponse<CommentResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Sorry! Post does not exist, check and try again."
                };
            }

            if (!string.IsNullOrWhiteSpace(commentRequestDto.ParentCommentId))
            {
                var parentComment = await commentRepository.FindOneAsync(c =>
                    c.Id == commentRequestDto.ParentCommentId && c.PostId == commentRequestDto.PostId);

                if (parentComment == null)
                {
                    return new ApiResponse<CommentResponseDto>
                    {
                        ResponseCode = (int)HttpStatusCode.NotFound,
                        Message = "Parent comment not found, check and try again."
                    };
                }
            }

            var newComment = new Storage.Entities.Comment
            {
                PostId = commentRequestDto.PostId,
                UserId = user.Id,
                Content = commentRequestDto.Content,
                ParentCommentId = commentRequestDto.ParentCommentId,
            };

            await commentRepository.AddAsync(newComment);

            var commentDto = newComment.Adapt<CommentResponseDto>();

            return new ApiResponse<CommentResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.Created,
                Message = "Comment added successfully",
                Data = commentDto
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while adding a comment to post {postId} -> Service: {service} -> Method: {method}.",
                commentRequestDto.PostId,
                nameof(CommentService), nameof(AddComment));

            return new ApiResponse<CommentResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while adding a comment, try again later."
            };
        }
    }
    
    public async Task<ApiResponse<ApiPagedResult<CommentNode>>> GetPostComments(string postId, BaseFilter baseFilter)
    {
        try
        {
            var post = await postRepository.GetByIdAsync(postId);

            if (post == null)
            {
                return new ApiResponse<ApiPagedResult<CommentNode>>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Post does not exist, check and try again."
                };
            }

            var queryable =  commentRepository.GetQueryable()
                .Include(c => c.User)
                .Include(c => c.Replies)
                .Include(c => c.ParentComment)
                .Where(c => c.PostId == postId);
        
            var comments = await queryable.OrderByDescending(p => p.CreatedAt)
                .GetPaged(baseFilter.PageNumber, baseFilter.PageSize);
            
            var commentDict = comments.Results.ToDictionary(c => c.Id, c => new CommentNode
            {
                CommentId = c.Id,
                UserId = c.UserId,
                Username = c.User.Username,
                ProfilePictureUrl = c.User.ProfilePictureUrl,
                Content = c.Content
            });

            foreach (var comment in comments.Results)
            {
                if (comment.ParentCommentId != null && commentDict.TryGetValue(comment.ParentCommentId, out var parent))
                {
                    parent.Replies.Add(commentDict[comment.Id]);
                }
            }

            var rootComments = comments.Results
                .Where(c => c.ParentCommentId == null)
                .Select(c => commentDict[c.Id])
                .ToList();


            foreach (var commentNode in commentDict.Values)
            {
                var likeCount = await postLikeRepository.CountAsync(cl => cl.CommentId == commentNode.CommentId);
                commentNode.LikeCount = likeCount;
            }
            
            var response = new ApiPagedResult<CommentNode>
            {
                Results = rootComments,
                UpperBound = comments.UpperBound,
                LowerBound = comments.LowerBound,
                PageIndex = comments.PageIndex,
                PageSize = comments.PageSize,
                TotalCount = comments.TotalCount,
                TotalPages = comments.TotalPages
            };

            return new ApiResponse<ApiPagedResult<CommentNode>>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Comments fetched successfully",
                Data = response,
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while fetching comments for post {postId} -> Service: {service} -> Method: {method}.",
                postId,
                nameof(CommentService), nameof(GetPostComments));

            return new ApiResponse<ApiPagedResult<CommentNode>>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while fetching comments, try again later."
            };
        }
    }

    public async Task<ApiResponse<CommentNode>> GetCommentWithReplies(string commentId)
    {
        try
        {
            var comment = await GetCommentById(commentId);

            if (comment == null)
            {
                return new ApiResponse<CommentNode>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = $"Comment with Id {commentId} not found."
                };
            }

            var commentDictionary = new Dictionary<string, CommentNode>();

            var commentNode = new CommentNode
            {
                CommentId = comment.Id,
                UserId = comment.UserId,
                Username = comment.User.Username,
                ProfilePictureUrl = comment.User.ProfilePictureUrl,
                Content = comment.Content,
                Replies = []
            };

            commentDictionary[comment.Id] = commentNode;

            var stack = new Stack<CommentNode>();
            stack.Push(commentNode);

            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();

                if (currentNode.CommentId == null) continue;
                
                var currentComment = await GetCommentById(currentNode.CommentId);

                if (currentComment == null)
                {
                    continue;
                }

                foreach (var reply in currentComment.Replies)
                {
                    var replyNode = new CommentNode
                    {
                        CommentId = reply.Id,
                        UserId = reply.UserId,
                        Username = reply.User.Username,
                        ProfilePictureUrl = reply.User.ProfilePictureUrl,
                        Content = reply.Content,
                        Replies = []
                    };

                    currentNode.Replies.Add(replyNode);

                    if (!commentDictionary.ContainsKey(reply.Id))
                    {
                        commentDictionary[reply.Id] = replyNode;
                    }

                    stack.Push(replyNode);
                }
            }

            var commentLikeCount = await postLikeRepository.CountAsync(pl => pl.CommentId == commentId);
            commentNode.LikeCount = commentLikeCount;
            
            return new ApiResponse<CommentNode>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Comment and its replies fetched successfully",
                Data = commentNode
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while fetching comment and replies for comment {commentId} -> Service: {service} -> Method: {method}.",
                commentId,
                nameof(CommentService), nameof(GetCommentWithReplies));

            return new ApiResponse<CommentNode>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while fetching comment and replies, try again later."
            };
        }
    }
    public async Task<ApiResponse<bool>> DeleteComment(string commentId, string? username)
    {
        try
        {
            var user = await userRepository.FindOneAsync(u => u.Username == username);
            
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User not found",
                };
            }
            
            var comment = await commentRepository.FindOneAsync(c => c.Id == commentId && c.UserId == user.Id);

            if (comment == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Comment not found",
                };
            }

            var deleteCommentResponse = await commentRepository.DeleteAsync(comment);

            if (deleteCommentResponse < 1)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Sorry! We could not delete comment, try again.",
                    Data = false
                };
            }

            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Comment and replies deleted successfully",
                Data = true
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while deleting comment {commentId} -> Service: {service} -> Method: {method}.",
                commentId,
                nameof(CommentService), nameof(DeleteComment));

            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while deleting the comment, try again later.",
                Data = false
            };
        }
    }

    private async Task<Storage.Entities.Comment?> GetCommentById(string commentId)
    {
        var comment = await commentRepository.GetQueryable()
            .Include(c => c.User)
            .Include(c => c.Replies)
            .ThenInclude(r => r.User)
            .Include(c => c.ParentComment)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        return comment;
    }
}
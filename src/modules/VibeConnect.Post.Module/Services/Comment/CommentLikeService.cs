using System.Net;
using Microsoft.EntityFrameworkCore;
using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Shared;
using VibeConnect.Shared.Extensions;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Post.Module.Services.Comment;

public class CommentLikeService(IBaseRepository<PostLike> postLikeRepository, IBaseRepository<Storage.Entities.Comment> commentRepository,
    IBaseRepository<User> userRepository, ILoggerAdapter<CommentLikeService> logger) : ICommentLikeService
{
    public async Task<ApiResponse<int>> HandleCommentLike(string commentId, string? username, bool isLike = false)
    {
        try
        {
            logger.LogInformation(
                "Received request to like/unlike comment {comment} by user {user} -> Service: {service} -> Method: {method}.",
                commentId,
                username,
                nameof(CommentLikeService),
                nameof(HandleCommentLike)
            );
            
            var existingComment = await commentRepository.GetByIdAsync(commentId);

            if (existingComment == null)
            {
                return new ApiResponse<int>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Comment not found"
                };
            }

            var user = await userRepository.FindOneAsync(x => x.Username == username);

            if (user == null)
            {
                return new ApiResponse<int>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = $"Sorry! User {username} does not exist, check and try again."
                };
            }
            
            var postId = existingComment.PostId;


            var existingLike = await postLikeRepository.FindOneAsync(pl => pl.CommentId == commentId && pl.UserId == user.Id);
            
            switch (isLike)
            {
                case true when existingLike == null:
                {
                    var newCommentLike = new PostLike
                    {
                        PostId = postId,
                        CommentId = commentId,
                        UserId = user.Id
                    };

                    var likeCommentResponse = await postLikeRepository.AddAsync(newCommentLike);

                    if (likeCommentResponse < 1)
                    {
                        return new ApiResponse<int>
                        {
                            ResponseCode = (int)HttpStatusCode.FailedDependency,
                            Message = "Sorry! We could not complete the like comment request, try again."
                        };
                    }

                    break;
                }
                case false when existingLike != null:
                {
                    var unlikeComment = await postLikeRepository.DeleteAsync(existingLike);

                    if (unlikeComment < 1)
                    {
                        return new ApiResponse<int>
                        {
                            ResponseCode = (int)HttpStatusCode.FailedDependency,
                            Message = "Could not complete the unlike comment request, try again."
                        };
                    }

                    break;
                }
            }

            var likeCount = await postLikeRepository.CountAsync(pl => pl.CommentId == commentId);

            return new ApiResponse<int>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Successful",
                Data = likeCount
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while handling comment {id} like/unlike for user {user} -> Service: {service} -> Method: {method}.",
                commentId,
                username,
                nameof(CommentLikeService), nameof(HandleCommentLike));
            
            return new ApiResponse<int>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened, try again later."
            };
        }
    }

    public async Task<ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>> GetUsersWhoLikedComment(string commentId, BaseFilter baseFilter)
    {
        try
        {
            logger.LogInformation("Get users who liked comment {commentId} -> Service: {service} -> Method: {method}.",
                commentId,
                nameof(CommentLikeService),
                nameof(GetUsersWhoLikedComment)
            );

            var post = await commentRepository.GetByIdAsync(commentId);

            if (post == null)
            {
                return new ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Sorry! Comment does not exist, check and try again."
                };
            }

            var query = postLikeRepository.GetQueryable()
                .Where(pl => pl.CommentId == commentId)
                .Include(pl => pl.User)
                .Select(pl => new UserLikedPostResponseDto
                {
                    Id = pl.User.Id,
                    Username = pl.User.Username,
                    FullName = pl.User.FullName,
                    Bio = pl.User.Bio,
                    ProfilePictureUrl = pl.User.ProfilePictureUrl
                });

            var pagedResult = await query.GetPaged(baseFilter.PageNumber, baseFilter.PageSize);

            return new ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Users who liked the post fetched successfully",
                Data = pagedResult
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while fetching users who liked comment {commentId} -> Service: {service} -> Method: {method}.",
                commentId,
                nameof(CommentLikeService), nameof(GetUsersWhoLikedComment));

            return new ApiResponse<ApiPagedResult<UserLikedPostResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while fetching users who liked the comment, try again later."
            };
        }
    }

}
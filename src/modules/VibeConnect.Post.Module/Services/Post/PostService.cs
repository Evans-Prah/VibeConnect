using System.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;
using VibeConnect.Post.Module.DTOs.Post;
using VibeConnect.Shared;
using VibeConnect.Shared.Extensions;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Post.Module.Services.Post;

public class PostService(
    IBaseRepository<Storage.Entities.Post> postRepository,
    ILoggerAdapter<PostService> logger,
    IBaseRepository<User> userRepository) : IPostService
{

    public async Task<ApiResponse<PostResponseDto>> CreatePost(string? username, PostRequestDto postRequestDto)
    {
        try
        {
            logger.LogInformation(
                "Create post for user {user} -> Service: {service} -> Method: {method}. Payload --> {request}",
                username,
                nameof(PostService),
                nameof(CreatePost),
                postRequestDto
            );

            var user = await userRepository.FindOneAsync(u => u.Username == username);

            if (user == null)
            {
                return new ApiResponse<PostResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = $"Sorry! User {username} does not exist, check and try again."
                };
            }

            var post = new Storage.Entities.Post
            {
                UserId = user.Id,
                Content = postRequestDto.Content,
                MediaContents = postRequestDto.MediaContents,
                Location = postRequestDto.Location
            };
            
            var createPost = await postRepository.AddAsync(post);

            if (createPost < 1)
            {
                return new ApiResponse<PostResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "We could not save user post, please try again"
                };
            }

            var postDto = post.Adapt<PostResponseDto>();

            return new ApiResponse<PostResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.Created,
                Message = "Post created successfully",
                Data = postDto
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while creating post for user {user} -> Service: {service} -> Method: {method}.", username,
                nameof(PostService), nameof(CreatePost));
            
            return new ApiResponse<PostResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while creating post, try again later."
            };
        }   
    }

    public async Task<ApiResponse<ApiPagedResult<PostResponseDto>>> GetUserPosts(BaseFilter baseFilter, string? username = null)
    {
        try
        {
            logger.LogInformation(
                "Get user {user} posts -> Service: {service} -> Method: {method}.",
                username,
                nameof(PostService),
                nameof(GetUserPosts)
            );

            var user = await userRepository.FindOneAsync(x => x.Username == username);

            if (user == null)
            {
                return new ApiResponse<ApiPagedResult<PostResponseDto>>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = $"Sorry! User {username} does not exist, check and try again."
                };
            }

            var query = postRepository.GetQueryable().AsNoTracking().Where(p => p.UserId == user.Id);

            var pagedResult = await query.OrderByDescending(p => p.CreatedAt)
                .GetPaged(baseFilter.PageNumber, baseFilter.PageSize);

            var posts = pagedResult.Results.Select(p => p.Adapt<PostResponseDto>()).ToList();

            var response = new ApiPagedResult<PostResponseDto>
            {
                Results = posts,
                UpperBound = pagedResult.UpperBound,
                LowerBound = pagedResult.LowerBound,
                PageIndex = pagedResult.PageIndex,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                TotalPages = pagedResult.TotalPages
            };

            return new ApiResponse<ApiPagedResult<PostResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "User posts fetched successfully",
                Data = response
            };

        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while fetching user {user} posts -> Service: {service} -> Method: {method}.", username,
                nameof(PostService), nameof(GetUserPosts));
            
            return new ApiResponse<ApiPagedResult<PostResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while fetching posts, try again later."
            };
        }
    }
    
    public async Task<ApiResponse<PostResponseDto>> GetUserPost(string postId, string? username = null)
    {
        try
        {
            logger.LogInformation(
                "Get user {user} post {postId} -> Service: {service} -> Method: {method}.",
                username,
                postId,
                nameof(PostService),
                nameof(GetUserPost)
            );

            var user = await userRepository.FindOneAsync(x => x.Username == username);

            if (user == null)
            {
                return new ApiResponse<PostResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = $"Sorry! User {username} does not exist, check and try again."
                };
            }

            var post = await postRepository.FindOneAsync(p => p.Id == postId && p.UserId == user.Id);

            if (post == null)
            {
                return new ApiResponse<PostResponseDto>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Sorry! Post does not exits, check and try again."
                };
            }

            var postDto = post.Adapt<PostResponseDto>();

            return new ApiResponse<PostResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "User post fetched successfully",
                Data = postDto
            };

        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occured while fetching user {user} post {id} -> Service: {service} -> Method: {method}.", 
                username,
                postId,
                nameof(PostService), nameof(GetUserPost));
            
            return new ApiResponse<PostResponseDto>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened while fetching post, try again later."
            };
        }
    }
}
using System.Net;
using VibeConnect.Shared;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Post.Module.Services.PostLikes;

public class PostLikeService(IBaseRepository<PostLike> postLikeRepository,
    IBaseRepository<Storage.Entities.Post> postRepository,
    ILoggerAdapter<PostLikeService> logger,
    IBaseRepository<User> userRepository) : IPostLikeService
{
    public async Task<ApiResponse<int?>> HandlePostLike(string postId, string? username, bool isLike = false)
    {
        try
        {
            logger.LogInformation(
                "Received request to like/unlike post {post} by user {user} -> Service: {service} -> Method: {method}.",
                postId,
                username,
                nameof(PostLikeService),
                nameof(HandlePostLike)
            );

            var validationError = ValidateInputs(postId, username);
            if (validationError != null)
            {
                return validationError;
            }

            var existingPost = await postRepository.GetByIdAsync(postId);

            if (existingPost == null)
            {
                return new ApiResponse<int?>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Post not found"
                };
            }

            var user = await userRepository.FindOneAsync(x => x.Username == username);

            if (user == null)
            {
                return new ApiResponse<int?>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = $"Sorry! User {username} does not exist, check and try again."
                };
            }

            var existingLike = await postLikeRepository.FindOneAsync(pl => pl.PostId == postId && pl.UserId == user.Id);

            switch (isLike)
            {
                case true when existingLike == null:
                {
                    var newPostLike = new PostLike
                    {
                        PostId = postId,
                        UserId = user.Id
                    };

                    var likePostResponse = await postLikeRepository.AddAsync(newPostLike);

                    if (likePostResponse < 1)
                    {
                        return new ApiResponse<int?>
                        {
                            ResponseCode = (int)HttpStatusCode.FailedDependency,
                            Message = "Sorry! We could not complete the like post request, try again."
                        };
                    }

                    break;
                }
                case false when existingLike != null:
                {
                    var unlikePost = await postLikeRepository.DeleteAsync(existingLike);

                    if (unlikePost < 1)
                    {
                        return new ApiResponse<int?>
                        {
                            ResponseCode = (int)HttpStatusCode.FailedDependency,
                            Message = "Could not complete the unlike post request, try again."
                        };
                    }

                    break;
                }
            }


            var likeCount = await postLikeRepository.CountAsync(pl => pl.PostId == postId);

            return new ApiResponse<int?>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Successful",
                Data = likeCount
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while handling post {id} like/unlike for user {user} -> Service: {service} -> Method: {method}.",
                postId,
                username,
                nameof(PostLikeService), nameof(HandlePostLike));
            
            return new ApiResponse<int?>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened, try again later."
            };
        }
    }

    private static ApiResponse<int?>? ValidateInputs(string postId, string? username)
    {
        if (string.IsNullOrWhiteSpace(postId))
        {
            return new ApiResponse<int?>
            {
                ResponseCode = (int)HttpStatusCode.BadRequest,
                Message = "Invalid postId"
            };
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            return new ApiResponse<int?>
            {
                ResponseCode = (int)HttpStatusCode.BadRequest,
                Message = "Invalid username"
            };
        }

        return null;
    }

}
using System.Net;
using Microsoft.EntityFrameworkCore;
using VibeConnect.Friendship.Module.DTOs;
using VibeConnect.Shared;
using VibeConnect.Shared.Extensions;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Friendship.Module.Services;

public class FriendshipService(IBaseRepository<Storage.Entities.Friendship> friendshipRepository, IBaseRepository<User> userRepository, ILoggerAdapter<FriendshipService> logger) : IFriendshipService
{
    public async Task<ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>> GetUserFollowers(string username, BaseFilter baseFilter)
    {
        try
        {
            var user = await userRepository.FindOneAsync(u => u.Username == username);
            if (user == null)
            {
                return new ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User not found, check and try again"
                };
            }

            var queryable = friendshipRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(f => f.FollowingId == user.Id);
            
            var followers = await queryable
                .OrderByDescending(f => f.FollowedAt)
                .GetPaged(baseFilter.PageNumber, baseFilter.PageSize);

            var followerDtos = new List<UserFollowerFollowingDto>();
            

            foreach (var follower in followers.Results)
            {
                var followerUser = await userRepository.GetByIdAsync(follower.FollowerId);
                if (followerUser == null) continue;

                var followerDto = new UserFollowerFollowingDto
                {
                    Username = followerUser.Username,
                    FullName = followerUser.FullName,
                    Bio = followerUser.Bio,
                    ProfilePictureUrl = followerUser.ProfilePictureUrl,
                    IsMutual = follower.IsMutual
                };

                followerDtos.Add(followerDto);
            }

            var response = new ApiPagedResult<UserFollowerFollowingDto>
            {
                Results = followerDtos,
                UpperBound = followers.UpperBound,
                LowerBound = followers.LowerBound,
                PageIndex = followers.PageIndex,
                PageSize = followers.PageSize,
                TotalCount = followers.TotalCount,
                TotalPages = followers.TotalPages
            };

            return new ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "User followers fetched successfully",
                Data = response
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while retrieving user followers -> Service: {service} -> Method: {method}.",
                nameof(FriendRequestService), nameof(GetUserFollowers));
            return new ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened when fetching user followers"
            };
        }
    }

    public async Task<ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>> GetUserFollowing(string username, BaseFilter baseFilter)
    {
        try
        {
            var user = await userRepository.FindOneAsync(u => u.Username == username);
            if (user == null)
            {
                return new ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User not found, check and try again"
                };
            }

            var queryable = friendshipRepository
                .GetQueryable()
                .AsNoTracking()
                .Where(f => f.FollowerId == user.Id);

            var followings = await queryable
                .OrderByDescending(f => f.FollowedAt)
                .GetPaged(baseFilter.PageNumber, baseFilter.PageSize);

            var followingDtos = new List<UserFollowerFollowingDto>();

            foreach (var following in followings.Results)
            {
                var followingUser = await userRepository.GetByIdAsync(following.FollowingId);
                if (followingUser == null) continue;

                var followingDto = new UserFollowerFollowingDto
                {
                    Username = followingUser.Username,
                    FullName = followingUser.FullName,
                    Bio = followingUser.Bio,
                    ProfilePictureUrl = followingUser.ProfilePictureUrl,
                    IsMutual = following.IsMutual
                };

                followingDtos.Add(followingDto);
            }

            var response = new ApiPagedResult<UserFollowerFollowingDto>
            {
                Results = followingDtos,
                UpperBound = followings.UpperBound,
                LowerBound = followings.LowerBound,
                PageIndex = followings.PageIndex,
                PageSize = followings.PageSize,
                TotalCount = followings.TotalCount,
                TotalPages = followings.TotalPages
            };      

            return new ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "User followings fetched successfully",
                Data = response
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while retrieving user followings -> Service: {service} -> Method: {method}.",
                nameof(FriendRequestService), nameof(GetUserFollowing));
            return new ApiResponse<ApiPagedResult<UserFollowerFollowingDto>>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened when fetching user followings"
            };
        }
    }
    
    public async Task<ApiResponse<bool>> UnfollowUserTransaction(string username, string followingUsername)
    {
        try
        {
            var user = await userRepository.FindOneAsync(u => u.Username == username);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User not found, check and try again"
                };
            }

            var followingUser = await userRepository.FindOneAsync(u => u.Username == followingUsername);
            if (followingUser == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Following user not found, check and try again"
                };
            }

            var friendship = await friendshipRepository.FindOneAsync(f =>
                f.FollowerId == user.Id && f.FollowingId == followingUser.Id);

            if (friendship == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Friendship not found, check and try again"
                };
            }

            await friendshipRepository.BeginTransactionAsync();
            try
            {
                var deleteFriendship = await friendshipRepository.DeleteAsync(friendship);
                if (deleteFriendship < 1)
                {
                    return new ApiResponse<bool>
                    {
                        ResponseCode = (int)HttpStatusCode.FailedDependency,
                        Message = $"Could not unfollow {followingUsername}, please try again"
                    };
                }
                
                var remainingFriendship = await friendshipRepository.FindOneAsync(f =>
                    f.FollowerId == followingUser.Id && f.FollowingId == user.Id);
                
                if (remainingFriendship != null)
                {
                    remainingFriendship.IsMutual = false;
                    await friendshipRepository.UpdateAsync(remainingFriendship);
                }

                user.TotalFollowing--;
                followingUser.TotalFollowers--;

                var updateUserResult = await userRepository.UpdateAsync(user);
                var updateFollowingUserResult = await userRepository.UpdateAsync(followingUser);

                if (updateUserResult < 1 || updateFollowingUserResult < 1)
                {
                    // Rollback the transaction if any update fails
                    await friendshipRepository.RollbackTransactionAsync(); 

                    return new ApiResponse<bool>
                    {
                        ResponseCode = (int)HttpStatusCode.FailedDependency,
                        Message = "Failed to update user data, please try again"
                    };
                }

                // Commit the transaction if all operations succeed
                await friendshipRepository.CommitTransactionAsync();

                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.OK,
                    Message = "User unfollowed successfully",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                // Rollback the transaction in case of any exception
                await friendshipRepository.RollbackTransactionAsync();

                logger.LogError(ex, "An error occurred while unfollowing user -> Service: {service} -> Method: {method}.",
                    nameof(FriendshipService), nameof(UnfollowUserTransaction));
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.InternalServerError,
                    Message = "Something bad happened when unfollowing user"
                };
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while unfollowing user -> Service: {service} -> Method: {method}.",
                nameof(FriendshipService), nameof(UnfollowUserTransaction));
            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened when unfollowing user"
            };
        }
    }
    
    
    public async Task<ApiResponse<bool>> UnfollowUser(string username, string followingUsername)
    {
        try
        {
            var user = await userRepository.FindOneAsync(u => u.Username == username);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User not found, check and try again"
                };
            }

            var followingUser = await userRepository.FindOneAsync(u => u.Username == followingUsername);
            if (followingUser == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Following user not found, check and try again"
                };
            }

            var friendship = await friendshipRepository.FindOneAsync(f =>
                f.FollowerId == user.Id && f.FollowingId == followingUser.Id);

            if (friendship == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = "You are not following this user"
                };
            }

            // Delete the friendship record
            var deleteFriendship = await friendshipRepository.DeleteAsync(friendship);
            if (deleteFriendship < 1)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = $"Could not unfollow {followingUsername}, please try again"
                };
            }

            // Check if there is a mutual friendship and update it
            var remainingFriendship = await friendshipRepository.FindOneAsync(f =>
                f.FollowerId == followingUser.Id && f.FollowingId == user.Id);

            if (remainingFriendship != null)
            {
                remainingFriendship.IsMutual = false;
                await friendshipRepository.UpdateAsync(remainingFriendship);

                followingUser.TotalFollowers--;
                await userRepository.UpdateAsync(followingUser);
            }
            else
            {
                followingUser.TotalFollowers--;
                await userRepository.UpdateAsync(followingUser);
            }

            // Update the TotalFollowing for the user who initiated the unfollow
            user.TotalFollowing--;
            await userRepository.UpdateAsync(user);

            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Successfully unfollowed user",
                Data = true
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while unfollowing user -> Service: {service} -> Method: {method}.",
                nameof(FriendRequestService), nameof(UnfollowUser));

            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened, please try again later."
            };
        }
    }
}
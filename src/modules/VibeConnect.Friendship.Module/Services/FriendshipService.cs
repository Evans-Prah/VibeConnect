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
            
            // var queryable = friendshipRepository
            //     .GetQueryable()
            //     .AsNoTracking()
            //     .Join(friendshipRepository.GetQueryable(), // Join with Friendship table itself
            //         f => f.FollowingId,
            //         f2 => f2.FollowerId,
            //         (f, f2) => new { f.FollowingId, f2.FollowerId, f.FollowedAt, f.IsMutual })
            //     .Where(joined => joined.FollowerId == user.Id); // Filter by user being followed (John)

           

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
}
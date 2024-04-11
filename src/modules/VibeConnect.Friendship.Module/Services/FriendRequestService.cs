using System.Net;
using Microsoft.EntityFrameworkCore;
using VibeConnect.Friendship.Module.DTOs;
using VibeConnect.Shared;
using VibeConnect.Shared.Extensions;
using VibeConnect.Shared.Models;
using VibeConnect.Storage.Entities;
using VibeConnect.Storage.Services;

namespace VibeConnect.Friendship.Module.Services;

public class FriendRequestService (IBaseRepository<FriendshipRequest> friendRequestRepository, IBaseRepository<Storage.Entities.Friendship> friendshipRepository,
    IBaseRepository<User> userRepository, ILoggerAdapter<FriendRequestService> logger) : IFriendRequestService
{
    private const int MaximumDailyFriendRequestLimit = 50;

    public async Task<ApiResponse<object>> SendFriendRequest(string? senderUsername,FriendRequestDto friendRequestDto)
    {
        try
        {

            var sender = await userRepository.FindOneAsync(u => u.Username == senderUsername);
            if (sender == null)
            {
                return new ApiResponse<object>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Sender not found, check and try again"
                }; 
            }
            
            var exceedsLimit = await ExceedsFriendRequestLimit(sender.Id);

            if (exceedsLimit)
            {
                return new ApiResponse<object>
                {
                    ResponseCode = (int)HttpStatusCode.TooManyRequests,
                    Message = "Daily friend request limit exceeded"
                };
            }

            
            var receiver = await userRepository.FindOneAsync(u => u.Username == friendRequestDto.ReceiverUsername);
            if (receiver == null)
            {
                return new ApiResponse<object>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Receiver not found, check and try again"
                };
            }

            if (senderUsername == receiver.Username)
            {
                return new ApiResponse<object>
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = "You can't send friend request to yourself"
                };
            }
            
            if (Enum.TryParse(receiver.AccountStatus, out AccountStatus status) && status != AccountStatus.Active)
            {
                return new ApiResponse<object> 
                {
                    ResponseCode = (int)HttpStatusCode.BadRequest,
                    Message = "Receiver account is not active at the moment to receive friend requests"
                };
            }

            var existingFriendRequest =
                await friendRequestRepository.FindOneAsync(fr =>
                    fr.SenderId == sender.Id && fr.ReceiverId == receiver.Id);

            var existingReversedFriendRequest = await friendRequestRepository.FindOneAsync(fr => 
                fr.SenderId == receiver.Id && fr.ReceiverId == sender.Id);
            
            if (existingFriendRequest != null || existingReversedFriendRequest != null)
            {
                return new ApiResponse<object>
                {
                    ResponseCode = (int)HttpStatusCode.Conflict,
                    Message = "Friend request already sent."
                };
            }
            
            var existingFriend = await friendshipRepository
                .FindOneAsync(f => (f.FollowerId == receiver.Id && f.FollowingId == sender.Id) || 
                                   (f.FollowerId == sender.Id && f.FollowingId == receiver.Id));
            
            if (existingFriend != null)
            {
                return new ApiResponse<object>
                {
                    ResponseCode = (int)HttpStatusCode.Conflict,
                    Message = "You are already friends with this user."
                };
            }

            var friendRequest = new FriendshipRequest
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
            };

            var friendRequestDbResponse = await friendRequestRepository.AddAsync(friendRequest);
            
            if (friendRequestDbResponse < 1)
            {
                return new ApiResponse<object>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Sorry! We could not send your friend request, try again.",
                };
            }
            
            return new ApiResponse<object>
            {
                ResponseCode = (int)HttpStatusCode.Created,
                Message = "Friend request sent successfully"
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while sending friend request from user -> Service: {service} -> Method: {method}.",
                nameof(FriendRequestService), nameof(SendFriendRequest));

            return new ApiResponse<object>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened, please try again later."
            };
        }
    }

    public async Task<ApiResponse<ApiPagedResult<FriendRequestResponseDto>>> GetFriendRequests(string? username, BaseFilter baseFilter, bool sentRequests)
    {
        try
        {
            var currentUser = await userRepository.FindOneAsync(u => u.Username == username);

            if (currentUser == null)
            {
                return new ApiResponse<ApiPagedResult<FriendRequestResponseDto>>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Could not retrieve user details, check and try again."
                };
            }

            IQueryable<FriendshipRequest> queryable;

            if (sentRequests)
            {
                queryable = friendRequestRepository
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(fr => fr.SenderId == currentUser.Id);
            }
            else
            {
                queryable = friendRequestRepository
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(fr => fr.ReceiverId == currentUser.Id);
            }

            var friendRequests = await queryable
                .OrderByDescending(fr => fr.RequestedAt)
                .GetPaged(baseFilter.PageNumber, baseFilter.PageSize);

            var friendRequestDtos = new List<FriendRequestResponseDto>();

            foreach (var friendRequest in friendRequests.Results)
            {
                FriendRequestUserDto userDto;
                if (sentRequests)
                {
                    var receiver = await userRepository.GetByIdAsync(friendRequest.ReceiverId);
                    if (receiver == null) continue;

                    userDto = new FriendRequestUserDto
                    {
                        Id = receiver.Id,
                        Username = receiver.Username,
                        FullName = receiver.FullName,
                        ProfilePictureUrl = receiver.ProfilePictureUrl
                    };
                }
                else
                {
                    var sender = await userRepository.GetByIdAsync(friendRequest.SenderId);
                    if (sender == null) continue;

                    userDto = new FriendRequestUserDto
                    {
                        Id = sender.Id,
                        Username = sender.Username,
                        FullName = sender.FullName,
                        ProfilePictureUrl = sender.ProfilePictureUrl
                    };
                }

                var friendRequestDto = new FriendRequestResponseDto
                {
                    Id = friendRequest.Id,
                    User = userDto
                };

                friendRequestDtos.Add(friendRequestDto);
            }

            var response = new ApiPagedResult<FriendRequestResponseDto>
            {
                Results = friendRequestDtos,
                UpperBound = friendRequests.UpperBound,
                LowerBound = friendRequests.LowerBound,
                PageIndex = friendRequests.PageIndex,
                PageSize = friendRequests.PageSize,
                TotalCount = friendRequests.TotalCount,
                TotalPages = friendRequests.TotalPages
            };

            return new ApiResponse<ApiPagedResult<FriendRequestResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Friend requests fetched successfully",
                Data = response
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while retrieving friend requests for user -> Service: {service} -> Method: {method}.",
                nameof(FriendRequestService), nameof(GetFriendRequests));
            return new ApiResponse<ApiPagedResult<FriendRequestResponseDto>>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened when fetching friend requests"
            };
        }
    }   

    public async Task<ApiResponse<bool>> ApproveFriendRequest(string requestId, string? username)
    {
        try
        {
            var request = await friendRequestRepository.GetByIdAsync(requestId);

            if (request == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Friend request not found",
                    Data = false
                };
            }

            var user = await userRepository.FindOneAsync(u => u.Username == username);

            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User not found"
                };
            }

            if (request.ReceiverId != user.Id)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You can only approve friend requests sent to you",
                    Data = false
                };
            }

            if (request.SenderId == user.Id)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.Forbidden,
                    Message = "You cannot approve your own friend request",
                    Data = false
                };
            }

            var friend = new Storage.Entities.Friendship
            {
                FollowerId = request.SenderId,
                FollowingId = user.Id,
                IsMutual = true
            };
            
            var addFriend = await friendshipRepository.AddAsync(friend);
            if (addFriend < 1)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened when approving friend requests"
                };
            }
            
            var deleteRequest = await friendRequestRepository.DeleteAsync(request);
            if (deleteRequest < 1)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened when approving friend requests"
                };
            }
            
            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Friend request approved successfully",
                Data = true
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while approving friend requests for user -> Service: {service} -> Method: {method}.",
                nameof(FriendRequestService), nameof(ApproveFriendRequest));
            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened when approving friend requests"
            };
        }
    }
    
    public async Task<ApiResponse<bool>> RejectFriendRequest(string requestId, string? username)
    {
        try
        {
            var request = await friendRequestRepository.GetByIdAsync(requestId);
            if (request == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Friend request not found",
                    Data = false
                };
            }

            var user = await userRepository.FindOneAsync(u => u.Username == username);
            if (user == null)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "User not found"
                };
            }

            if (request.ReceiverId != user.Id)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.Unauthorized,
                    Message = "You can only reject friend requests sent to you friend request",
                    Data = false
                };
            }

            if (request.SenderId == user.Id)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.Forbidden,
                    Message = "You cannot reject your own friend request",
                    Data = false
                };
            }
            
            var friendship = new Storage.Entities.Friendship
            {
                FollowerId = request.SenderId,
                FollowingId = user.Id,
                IsMutual = false
            };
        
            var addFriendship = await friendshipRepository.AddAsync(friendship);
            if (addFriendship < 1)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened when rejecting friend requests"
                };
            }
            
            var deleteRequest = await friendRequestRepository.DeleteAsync(request);
            if (deleteRequest < 1)
            {
                return new ApiResponse<bool>
                {
                    ResponseCode = (int)HttpStatusCode.FailedDependency,
                    Message = "Something bad happened when rejecting friend requests"
                };
            }
            
            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.OK,
                Message = "Friend request rejected successfully",
                Data = true
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while rejecting friend requests for user -> Service: {service} -> Method: {method}.",
                nameof(FriendRequestService), nameof(RejectFriendRequest));
            return new ApiResponse<bool>
            {
                ResponseCode = (int)HttpStatusCode.InternalServerError,
                Message = "Something bad happened when rejecting friend requests"
            };
        }           
    }

    private async Task<bool> ExceedsFriendRequestLimit(string senderId)
    {
        var timePeriod = TimeSpan.FromHours(24);
        var cutOffTime = DateTimeOffset.UtcNow.Subtract(timePeriod);

        var requests = await friendRequestRepository.CountAsync(fr => 
            fr.SenderId == senderId && fr.RequestedAt >= cutOffTime);

        return requests >= MaximumDailyFriendRequestLimit;

    }

}
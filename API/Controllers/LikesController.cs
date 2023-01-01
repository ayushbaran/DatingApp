using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController: BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
            
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var SourceUserId = User.GetUserId();
            var likedUser = await _userRepository.GetUserByUsernameAsync(username);
            var SourceUser = await _likesRepository.GetUserWithLikes(SourceUserId);

            if(likedUser == null) return NotFound();

            if(SourceUser.UserName == username) return BadRequest("Cannot like self");

            var userLike = await _likesRepository.GetUserLike(SourceUserId, likedUser.Id);
            if(userLike != null) return BadRequest("Already liked");

            userLike = new UserLike
            {
                SourceUserId = SourceUserId,
                TargetUserId = likedUser.Id
            };

            SourceUser.LikedUsers.Add(userLike);

            if(await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Error Liking user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}
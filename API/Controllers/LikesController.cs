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
        private readonly IUnitOfWork _uow;
        public LikesController(IUnitOfWork uow)
        {            
            _uow = uow;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var SourceUserId = User.GetUserId();
            var likedUser = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var SourceUser = await _uow.LikesRepository.GetUserWithLikes(SourceUserId);

            if(likedUser == null) return NotFound();

            if(SourceUser.UserName == username) return BadRequest("Cannot like self");

            var userLike = await _uow.LikesRepository.GetUserLike(SourceUserId, likedUser.Id);
            if(userLike != null) return BadRequest("Already liked");

            userLike = new UserLike
            {
                SourceUserId = SourceUserId,
                TargetUserId = likedUser.Id
            };

            SourceUser.LikedUsers.Add(userLike);

            if(await _uow.Complete()) return Ok();

            return BadRequest("Error Liking user");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _uow.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}
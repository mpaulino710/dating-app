using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize] // Ensure that the user is authenticated
    public class MembersController(IMemberRepository memberRepository,
        IPhotoServices photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers([FromQuery]MemberParams memberParams)
        {
            memberParams.CurrentMemberId = User.GetMemberId(); // Get the current member ID from the claims
            return Ok(await memberRepository.GetMembersAsync(memberParams));
        }

        [HttpGet("{id}")] //localhost:5001/api/members/{id}
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await memberRepository.GetMemberByIdAsync(id);
            if (member == null) return NotFound();
            return member;
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IReadOnlyList<Photo>>> GetPhotosForMember(string id)
        {
            var photos = await memberRepository.GetPhotosForMemberAsync(id);
            //if (photos == null || !photos.Any()) return NotFound();
            return Ok(photos);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var memberId = User.GetMemberId(); // Get the member ID from the claims

            var member = await memberRepository.GetMemberForUpdate(memberId);
            if (member == null)
            {
                return NotFound("Member not found.");
            }

            // Update the member's properties with the values from the DTO
            member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDto.Description ?? member.Description;
            member.City = memberUpdateDto.City ?? member.City;
            member.Country = memberUpdateDto.Country ?? member.Country;

            member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName;
            // memberRepository.Update(member);

            if (await memberRepository.SaveAllAsync())
            {
                return NoContent(); // Return 204 No Content on successful update
            }
            else
            {
                return BadRequest("Failed to update member.");
            }

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
        {
            var member = await memberRepository.GetMemberForUpdate(User.GetMemberId());
            if (member == null)
            {
                return NotFound("Member not found.");
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var uploadResult = await photoService.UploadPhotoAsync(file);

            if (uploadResult.Error != null)
            {
                return BadRequest($"Error uploading photo: {uploadResult.Error.Message}");
            }

            var photo = new Photo
            {
                Url = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
                MemberId = User.GetMemberId()
            };

            if (member.ImageUrl == null)
            {
                member.ImageUrl = photo.Url;
                member.User.ImageUrl = photo.Url;
            }

            member.Photos.Add(photo);

            if (await memberRepository.SaveAllAsync())
            {
                return photo; // Return the newly added photo
            }
            else
            {
                return BadRequest("Failed to add photo.");
            }

        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var member = await memberRepository.GetMemberForUpdate(User.GetMemberId());
            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

            if (member.ImageUrl == photo?.Url || photo == null)
            {
                return BadRequest("Cannot set this main image");
            }

            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;

            if (await memberRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var member = await memberRepository.GetMemberForUpdate(User.GetMemberId());
            if (member == null) return BadRequest("Cannot get member from token");

            var photo = member.Photos.SingleOrDefault(x => x.Id == photoId);

            if (photo == null || photo.Url == member.ImageUrl)
            {
                return BadRequest("This photo cannot be deleted");
            }

            if (photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            member.Photos.Remove(photo);

            if (await memberRepository.SaveAllAsync()) return Ok();

            return BadRequest("Probem deleting this photo");
        }
    }
}

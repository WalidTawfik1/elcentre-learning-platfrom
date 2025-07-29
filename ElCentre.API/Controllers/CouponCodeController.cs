using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using ElCentre.Core.Sharing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElCentre.API.Controllers
{
    [ApiController]
    public class CouponCodeController : BaseController
    {
        public CouponCodeController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }
        /// <summary>
        /// Gets all coupon codes.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Instructor")]
        [HttpGet("get-coupon-codes")]
        public async Task<IActionResult> GetCouponCodes()
        {
            var couponCodes = await work.CouponCodeRepository.GetAllAsync();
            if (couponCodes == null || !couponCodes.Any())
            {
                return NotFound("No coupon codes found.");
            }
            return Ok(couponCodes);
        }

        /// <summary>
        /// Gets a specific coupon code by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Instructor")]
        [HttpGet("get-coupon-code/{id}")]
        public async Task<IActionResult> GetCouponCode(int id)
        {
            var couponCode = await work.CouponCodeRepository.GetByIdAsync(id);
            if (couponCode == null)
            {
                return NotFound($"Coupon code with ID {id} not found.");
            }
            return Ok(couponCode);
        }

        /// <summary>
        /// Adds a new coupon code.
        /// </summary>
        /// <param name="addCouponCodeDTO"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Instructor")]
        [HttpPost("add-coupon-code")]
        public async Task<IActionResult> AddCouponCode([FromForm] AddCouponCodeDTO addCouponCodeDTO)
        {
            try
            {
                if (addCouponCodeDTO == null)
                {
                    return BadRequest(new APIResponse(400, "Coupon code data cannot be null."));
                }
                var existingCoupon = await work.CouponCodeRepository.CouponCodeExists(addCouponCodeDTO.Code);
                if (existingCoupon)
                {
                    return BadRequest(new APIResponse(400, "Coupon code already exists, create another code."));
                }

                var couponCode = mapper.Map<CouponCode>(addCouponCodeDTO);
                await work.CouponCodeRepository.AddAsync(couponCode);
                return Ok(new APIResponse(200, "Coupon code added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

        /// <summary>
        /// Updates an existing coupon code.
        /// </summary>
        /// <param name="couponCode"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Instructor")]
        [HttpPut("update-coupon-code")]
        public async Task<IActionResult> UpdateCouponCode([FromForm] CouponCode couponCode)
        {
            try
            {
                if (couponCode == null)
                {
                    return BadRequest(new APIResponse(400, "Coupon code data cannot be null."));
                }
                
                await work.CouponCodeRepository.UpdateAsync(couponCode);
                return Ok(new APIResponse(200, "Coupon code updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }

        [Authorize(Roles = "Admin, Instructor")]
        [HttpDelete("delete-coupon-code/{id}")]
        public async Task<IActionResult> DeleteCouponCode(int id)
        {
            var couponCode = await work.CouponCodeRepository.GetByIdAsync(id);
            if (couponCode == null)
            {
                return NotFound($"Coupon code with ID {id} not found.");
            }
            await work.CouponCodeRepository.DeleteAsync(id);
            return Ok(new APIResponse(200,$"Coupon code with ID {id} has been deleted successfully."));
        }

        [Authorize]
        [HttpGet("apply-coupon-code")]
        public async Task<IActionResult> ApplyCouponCode(string code, int courseId)
        {
            var studentId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(studentId))
            {
                return Unauthorized("User not authenticated.");
            }
            try
            {
                var totalAmount = await work.CouponCodeRepository.GetFinalPrice(code, studentId, courseId);
                return Ok(new APIResponse(200, $"{totalAmount}"));
            }
            catch (Exception ex)
            {
                return BadRequest(new APIResponse(400, ex.Message));
            }
        }
    }
}

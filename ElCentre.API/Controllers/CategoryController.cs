using AutoMapper;
using ElCentre.API.Helper;
using ElCentre.Core.DTO;
using ElCentre.Core.Entities;
using ElCentre.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElCentre.API.Controllers
{
    public class CategoryController : BaseController
    {
        public CategoryController(IUnitofWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Get All Categories
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-all-categories")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await work.CategoryRepository.GetAllAsync();
                if (categories == null)
                {
                    return BadRequest(new APIResponse(400));
                }
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Get Category By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("get-category-by-id/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await work.CategoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return BadRequest(new APIResponse(400));
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Add Category by Admin
        /// </summary>
        /// <param name="addCategoryDTO"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("add-category")]
        public async Task<IActionResult> Add(AddCategoryDTO addCategoryDTO)
        {
            try
            {
                var category = mapper.Map<Category>(addCategoryDTO);
                await work.CategoryRepository.AddAsync(category);
                return Ok(new APIResponse(200, "Category added successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Update Category by Admin
        /// </summary>
        /// <param name="categoryDTO"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("update-category")]
        public async Task<IActionResult> Update(CategoryDTO categoryDTO)
        {
            try
            {
                var category = mapper.Map<Category>(categoryDTO);
                await work.CategoryRepository.UpdateAsync(category);
                return Ok(new APIResponse(200, "Category updated successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Delete Category by Admin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-category/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await work.CategoryRepository.DeleteAsync(id);
                return Ok(new APIResponse(200, "Category removed successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

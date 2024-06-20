using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.ServiceContracts.PetContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AlertaPatitasAPIUI.Controllers.PetsControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetController : ControllerBase
    {
        private readonly IAddPetService _addPetService;
        private readonly IGetPetService _getPetService;
        private readonly IUpdatePetService _updatePetService;
        private readonly IDeletePetService _deletePetService;

        public PetController(
            IAddPetService addPetService,
            IGetPetService getPetService,
            IUpdatePetService updatePetService,
            IDeletePetService deletePetService,
            IHttpContextAccessor httpContextAccessor)
        {
            _addPetService = addPetService;
            _getPetService = getPetService;
            _updatePetService = updatePetService;
            _deletePetService = deletePetService;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("add-pet")]
        public async Task<IActionResult> AddPet([FromBody] AddPetDTO addPetDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Invalid request");
            }

            var result = await _addPetService.AddPet(addPetDTO);

            if (result.Success)
            {
                return Ok(result.Details);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        //Endpoint for admins 
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("add-pet-admin")]
        public async Task<IActionResult> AddPetAdmin([FromBody] AddPetDTO addPetDTO)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest("Invalid request");
            }

            var result = await _addPetService.AddPet(addPetDTO);

            if (result.Success)
            {
                return Ok(result.Details);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpDelete("delete-pet/{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var result = await _deletePetService.DeletePet(id);

            if (result)
            {
                return Ok("Pet deleted successfully");
            }
            else
            {
                return NotFound("Pet not found");
            }
        }

        [AllowAnonymous]
        [HttpGet("get-pets-by-type/{type}")]
        public async Task<IActionResult> GetPetsByType(string type)
        {
            var pets = await _getPetService.GetPetByType(type);

            if (pets.Details != null && pets.Details.Count > 0)
            {
                return Ok(pets);
            }
            else
            {
                return NotFound("No pets found for the specified type");
            }
        }

        [AllowAnonymous]
        [HttpGet("get-pets-by-city/{city}")]
        public async Task<IActionResult> GetPetsByCity(string city)
        {
            var pets = await _getPetService.GetPetsByCity(city);

            if (pets.Details != null && pets.Details.Count > 0)
            {
                return Ok(pets);
            }
            else
            {
                return NotFound("No pets found for the specified city");
            }
        }

        [AllowAnonymous]
        [HttpGet("get-pets-by-shelter/{name}")]
        public async Task<IActionResult> GetPetsByShelter(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Shelter name cannot be null or empty");
            }
            var pets = await _getPetService.GetPetsByShelter(name);

            if (pets.Details != null && pets.Details.Count > 0)
            {
                return Ok(pets);
            }
            else
            {
                return NotFound("No pets found for the specified shelter");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-user-pets")]
        public async Task<IActionResult> GetUserPets()
        {
            

            var pets = await _getPetService.GetUserPets();

            if (pets != null && pets.Details != null && pets.Details.Count > 0 )
            {
                return Ok(pets);
            }
            else
            {
                return NotFound("No pets found for the user");
            }
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("update-pet")]
        public async Task<IActionResult> UpdatePet([FromBody] UpdatePetDTO pet)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request");
            }

            var result = await _updatePetService.UpdatePet(pet);

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound("Pet not found or update failed");
            }
        }
    }
}



global using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Client;


namespace dotnet_rpg.Services.CharacterService
{
    public class CharacterService : ICharacterService
    {
        private readonly IHttpContextAccessor _httpContextAccessor ;
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccesor)
        {
            _httpContextAccessor = httpContextAccesor;
            _context = context;
            _mapper = mapper;
        }
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User
            .FindFirstValue(ClaimTypes.NameIdentifier)!);

        public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
        {
            var serviceResponce = new ServiceResponse<List<GetCharacterDto>>();
            var character = _mapper.Map<Character>(newCharacter);
            character.user = await _context.Users.FirstOrDefaultAsync(u => u.id == GetUserId());
          
            _context.Characters.Add(character);
            await _context.SaveChangesAsync();

           serviceResponce.Data =await _context.Characters
                    .Where(c => c.user!.id == GetUserId())
                    .Select(c => _mapper.Map<GetCharacterDto>(c))
                    .ToListAsync();
           return serviceResponce;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> DeleteCharacter(int id)
        {
             var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();

            try {
            var character = await _context.Characters.FirstOrDefaultAsync(c => c.Id == id && c.user!.id == GetUserId());
            if(character is null)
            throw new Exception($"Character with Id '{id}' not found.");

            _context.Characters.Remove(character);
            await _context.SaveChangesAsync();

            serviceResponse.Data = await _context.Characters
            .Where(c => c.user!.id == GetUserId())
            .Select(c => _mapper.Map<GetCharacterDto>(c)).ToListAsync();
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
        {
            var serviceResponce = new ServiceResponse<List<GetCharacterDto>>();
            var dbCharacters = await _context.Characters
            .Include(c => c.weapon)
            .Include(c => c.Skills)
            .Where(c => c.user!.id == GetUserId()).ToListAsync();
            serviceResponce.Data = dbCharacters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
            return serviceResponce;
        }

        public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
        {
             var serviceResponce = new ServiceResponse<GetCharacterDto>();
           var dbCharacter = await _context.Characters
           .Include(c => c.weapon)
           .Include(c => c.Skills)
           .FirstOrDefaultAsync(c => c.Id == id && c.user!.id == GetUserId());

           serviceResponce.Data = _mapper.Map<GetCharacterDto>(dbCharacter);
           return serviceResponce;

        //    if(character is not null)
        //    return character;
        //    throw new Exception("character not found");
        }

        public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updateCharacter)
        {
            var serviceResponse = new ServiceResponse<GetCharacterDto>();

            try {
            var character = await _context.Characters
            .Include(c => c.user)
            .FirstOrDefaultAsync(c => c.Id == updateCharacter.Id);
            if(character is null || character.user!.id != GetUserId())
            throw new Exception($"Character with Id '{updateCharacter.Id}' not found.");

            _mapper.Map(updateCharacter, character);
            
            character.Name = updateCharacter.Name;
            character.HitPoints = updateCharacter.HitPoints;
            character.Intelligence = updateCharacter.Intelligence;
            character.Strength = updateCharacter.Strength;
            character.Defense = updateCharacter.Defense;
            character.Class = updateCharacter.Class;

            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch(Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
        {
            var response = new ServiceResponse<GetCharacterDto>();
            try
            {
                var character = await _context.Characters
                    .Include(c => c.weapon)
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId &&
                        c.user!.id == GetUserId());

                if (character is null)
                {
                    response.Success = false;
                    response.Message = "Character not found.";
                    return response;
                }

                var skill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);
                if (skill is null)
                {
                    response.Success = false;
                    response.Message = "Skill not found.";
                    return response;
                }

                character.Skills!.Add(skill);
                await _context.SaveChangesAsync();
                response.Data = _mapper.Map<GetCharacterDto>(character);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    
    }
}
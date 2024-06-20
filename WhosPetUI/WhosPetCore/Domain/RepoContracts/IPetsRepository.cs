using WhosPetCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhosPetCore.Domain.RepoContracts
{
    public interface IPetsRepository
    {

        public Task<List<Pets>> GetPetsByShelter(string name);

        public Task<List<Pets>> GetPetsByCity(string city);

        public Task<List<Pets>> GetUserPets(UserProfile user);
        public Task<List<Pets>> GetPetByType(string type);
        public Task<int> AddPet(Pets pet);

        public Task<bool> UpdatePet(Pets pet);

        public Task<bool> DeletePet(int id);

   
    }
}

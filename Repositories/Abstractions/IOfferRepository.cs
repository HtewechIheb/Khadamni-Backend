using Project_X.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project_X.Repositories.Abstractions
{
    public interface IOfferRepository
    {
        public Task<IEnumerable<Offer>> GetOffers();
        public Task<Offer> GetOfferById(long id);
        public Task<bool> AddOffer(Offer offer);
        public Task<bool> UpdateOffer(Offer offer);
        public Task<bool> DeleteOffer(long id);
    }
}

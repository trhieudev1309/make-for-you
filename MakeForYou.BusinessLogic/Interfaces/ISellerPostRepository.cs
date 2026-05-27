using MakeForYou.BusinessLogic.Entities;

namespace MakeForYou.BusinessLogic.Interfaces
{
    public interface ISellerPostRepository
    {
        /// <summary>Returns all posts for a seller, newest first.</summary>
        Task<List<SellerPost>> GetBySellerIdAsync(long sellerId);

        /// <summary>Returns a single post by its PK.</summary>
        Task<SellerPost?> GetByIdAsync(long postId);

        /// <summary>Persists a new post and returns it with the generated PostId.</summary>
        Task<SellerPost> AddAsync(SellerPost post);

        /// <summary>Saves changes to an already-tracked post entity.</summary>
        Task UpdateAsync(SellerPost post);

        /// <summary>
        /// Deletes the post only when sellerId matches the post's owner.
        /// Returns false when the post does not exist or the caller is not the owner.
        /// </summary>
        Task<bool> DeleteAsync(long postId, long sellerId);
    }
}

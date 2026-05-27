using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface ISellerPostService
    {
        /// <summary>Returns all posts for the given seller, newest first.</summary>
        Task<List<SellerPost>> GetPostsBySellerAsync(long sellerId);

        /// <summary>Returns a single post with seller info included.</summary>
        Task<SellerPost?> GetPostByIdAsync(long postId);

        /// <summary>
        /// Creates a new post for the seller.
        /// Optionally saves an uploaded image to wwwroot/uploads/posts/.
        /// </summary>
        Task<AuthResult> CreatePostAsync(long sellerId, CreateSellerPostRequest req);

        /// <summary>
        /// Updates an existing post. Only the seller who owns it may update.
        /// If a new image is provided the old file is replaced.
        /// </summary>
        Task<AuthResult> UpdatePostAsync(long postId, long sellerId, UpdateSellerPostRequest req);

        /// <summary>
        /// Deletes a post. Only the seller who owns it may delete.
        /// </summary>
        Task<AuthResult> DeletePostAsync(long postId, long sellerId);
    }
}

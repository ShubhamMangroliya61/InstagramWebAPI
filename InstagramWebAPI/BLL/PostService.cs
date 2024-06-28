using DataAccess.CustomModel;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;
using InstagramWebAPI.Interface;
using Microsoft.EntityFrameworkCore;

namespace InstagramWebAPI.BLL
{
    public class PostService: IPostService
    {
        public readonly ApplicationDbContext _dbcontext;
        public PostService(ApplicationDbContext db)
        {
            _dbcontext = db;
        }
        /// <summary>
        /// Creates or updates a post asynchronously.
        /// </summary>
        /// <param name="model">The DTO containing post data.</param>
        /// <returns>A PostResponseDTO object representing the created or updated post.</returns>
        public async Task<PostResponseDTO> CreatePostAsync(CreatePostDTO model)
        {
            Post post = await _dbcontext.Posts.FirstOrDefaultAsync(m => m.PostId == model.PostId && m.IsDeleted == false) ?? new();


            post.Caption = model.Caption;
            post.Location = model.Location;


            if (model.PostId > 0)
            {
                post.ModifiedDate = DateTime.Now;
                _dbcontext.Posts.Update(post);
            }
            else
            {
                post.CreatedDate = DateTime.Now;
                post.UserId = model.UserId;
                if (model.PostType == "Post")
                {
                    post.PostTypeId = 4;
                }
                else if (model.PostType == "Reel")
                {
                    post.PostTypeId = 3;
                }

                await _dbcontext.Posts.AddAsync(post);
            }
            await _dbcontext.SaveChangesAsync();


            var medias = new List<Media>();
            var postMappings = new List<PostMapping>();
            if (model.PostId == 0)
            {
                foreach (var file in model.File)
                {
                    var mediaType = Path.GetExtension(file.FileName).TrimStart('.');
                    string userId = model.UserId.ToString();
                    string path = "";

                    if (model.PostType == "Post")
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "Post");
                    }
                    else if (model.PostType == "Reel")
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "Reel");
                    }

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path ?? string.Empty);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(path ?? string.Empty, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    string mediaURL = Path.Combine("content", "User", userId, model.PostType ?? string.Empty, fileName);

                    int mediaTypeId = 0;

                    if (file.ContentType.Contains("image"))
                    {
                        mediaTypeId = 1;
                    }
                    else if (file.ContentType.Contains("video"))
                    {
                        mediaTypeId = 2;
                    }
                    var postMapping = new PostMapping
                    {
                        PostId = post.PostId,
                        MediaTypeId = mediaTypeId,
                        MediaUrl = mediaURL,
                        MediaName = fileName,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,

                    };

                    postMappings.Add(postMapping);
                    await _dbcontext.SaveChangesAsync();
                    var media = new Media
                    {
                        PostMappingId = postMapping.PostMappingId,
                        MediaType = file.ContentType,
                        MediaURL = mediaURL,
                        MediaName = fileName,
                    };
                    medias.Add(media);
                }

                await _dbcontext.PostMappings.AddRangeAsync(postMappings);
                await _dbcontext.SaveChangesAsync(); // Save all mappings in one go
            }

            // Prepare response DTO
            var responseDTO = new PostResponseDTO
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Caption = post.Caption,
                Location = post.Location,
                PostType = model.PostType,
                Medias = medias
            };
            return responseDTO;
        }

        public async Task<PaginationResponceModel<PostResponseDTO>> GetPostsListByIdAsync(RequestDTO<PostListRequestDTO> model)
        {
            IQueryable<PostResponseDTO> posts = _dbcontext.Posts
                .Include(m => m.PostMappings)
                .ThenInclude(m => m.MediaType)
                .Where(m => m.IsDeleted == false && (model.Model.PostType == "Post" ? m.PostTypeId == 4 : m.PostTypeId == 3) && m.UserId == model.Model.UserId)
                .OrderByDescending(p => p.CreatedDate)
                .Select(post => new PostResponseDTO
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    Caption = post.Caption,
                    Location = post.Location,
                    PostType = post.PostTypeId == 3 ? "Reel" : "Post",
                    Medias = post.PostMappings.Select(m => new Media
                    {
                        PostMappingId = m.PostMappingId,
                        MediaType = m.MediaTypeId == 1 ? "Images" : "Video",
                        MediaURL = m.MediaUrl,
                        MediaName = m.MediaName
                    }).ToList()
                });

            int totalRecords = await posts.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / model.PageSize);

            List<PostResponseDTO> postResponses = await posts
                .Skip((model.PageNumber - 1) * model.PageSize)
                .Take(model.PageSize)
                .ToListAsync();

            return new PaginationResponceModel<PostResponseDTO>
            {
                Totalrecord = totalRecords,
                PageSize = model.PageSize,
                PageNumber = model.PageNumber,
                RequirdPage = requiredPages,
                Record = postResponses
            };
        }

        public async Task<bool> DetelePostAsync(long postId)
        {
            Post? post = await _dbcontext.Posts.FirstOrDefaultAsync(m => m.PostId == postId && m.IsDeleted == false);
            if (post != null)
            {
                post.IsDeleted = true;
                post.ModifiedDate = DateTime.Now;

                _dbcontext.Posts.Update(post);
                await _dbcontext.SaveChangesAsync();

                return true;
            }
            return false;
        }
        public async Task<bool> LikeAndUnlikePostAsync(LikePostDTO model)
        {
            Like? like = await _dbcontext.Likes.FirstOrDefaultAsync(m => m.UserId == model.userId && m.PostId == model.postId);

            if (like != null)
            {
                if (model.isLike != like.IsLike)
                {
                    like.IsLike = model.isLike;
                    like.IsDeleted = !model.isLike;
                    like.ModifiedDate = DateTime.Now;

                    _dbcontext.Likes.Update(like);
                    await _dbcontext.SaveChangesAsync();
                }
                return true;
            }
            else
            {
                if (model.isLike)
                {
                    Like newLike = new()
                    {
                        UserId = model.userId,
                        PostId = model.postId,
                        IsLike = true,
                        IsDeleted = false,
                        CreatedDate = DateTime.Now,
                    };

                    await _dbcontext.Likes.AddAsync(newLike);
                    await _dbcontext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }
    }
}

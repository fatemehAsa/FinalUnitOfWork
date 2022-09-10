using Infrastructure.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using Models.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Application.Admin.Category.Queries
{
    public class CategoryQueriesHandler : IRequestHandler<GetCategoryByIdQueries, Models.Models.Category>,
        IRequestHandler<GetCategoryListQueries, IEnumerable<Models.Models.Category>>,
        IRequestHandler<IsExistCategoryQueries, bool>, IRequestHandler<IsExistCategoryInPostQueries, bool>, IRequestHandler<GetCategoryForEditPicQueries, Infrastructure.DTOs.PicCategoryViewModel>
    {
        //CustomContext _context;
        private readonly UnitOfWork _unitOfWork;
        public CategoryQueriesHandler(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Models.Models.Category>> Handle(GetCategoryListQueries request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CategoryRepository.GetWithTracking().ToListAsync(cancellationToken);
        }


        public async Task<Models.Models.Category> Handle(GetCategoryByIdQueries request, CancellationToken cancellationToken)
        {
            //var cat = await _context.tbl_Categories.FirstOrDefaultAsync(c => c.GenreId == request.GenreId, cancellationToken);
            var cat = await _unitOfWork.CategoryRepository.GetByID(request.GenreId, cancellationToken);
            return cat;
        }

        public async Task<bool> Handle(IsExistCategoryQueries request, CancellationToken cancellationToken)
        {
            //return await _context.tbl_Categories.AnyAsync(c => c.GenreId == request.CategoryId, cancellationToken);
            return await _unitOfWork.CategoryRepository.IsExist(request.CategoryId, cancellationToken);
        }

        public async Task<bool> Handle(IsExistCategoryInPostQueries request, CancellationToken cancellationToken)
        {
            //return await _context.tbl_Posts.AnyAsync(c=>c.GenreId==request.Id);
            return await _unitOfWork.PostRepository.GetAsNoTracking(el => el.GenreId == request.Id).AnyAsync(cancellationToken);
        }

        public async Task<PicCategoryViewModel> Handle(GetCategoryForEditPicQueries request, CancellationToken cancellationToken)
        {
            var imgName = await _unitOfWork.CategoryRepository
                .GetAsNoTracking(el => el.GenreId == request.Id)
                .Select(s => s.ImageName)
                .FirstOrDefaultAsync(cancellationToken);
            

            var width = 0;
            var height = 0;

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(),$"wwwroot/images/img/{imgName}");
            using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var image = Image.FromStream(fileStream, false, false))
                {
                     height = image.Height;
                     width = image.Width;
                }
            }

            return await _unitOfWork.CategoryRepository.GetAsNoTracking(el => el.GenreId == request.Id)
                .Select(s => new PicCategoryViewModel()
                {
                    Id = s.GenreId,
                    AvatarName = s.ImageName,
                    Width=width,
                    Height=height
                }).FirstOrDefaultAsync(cancellationToken);

        
        }
    }
}

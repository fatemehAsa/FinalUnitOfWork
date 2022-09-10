using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Application.Application.Admin.Category.Queries
{
   public class GetCategoryForEditPicQueries:IRequest<Infrastructure.DTOs.PicCategoryViewModel>
    {
        public GetCategoryForEditPicQueries(int catId)
        {
            Id = catId;
        }

        public int Id { get; set; }
    }

}

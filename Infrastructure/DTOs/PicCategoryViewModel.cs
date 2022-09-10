using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTOs
{
   public class PicCategoryViewModel
    {
        public int Id { get; set; }
        public string AvatarName { get; set; }

        [DisplayName("(px) عرض تصویر")]
        [Range(50, 2000, ErrorMessage = "{0} می بایست در بازه 2000-50 تعیین شود ")]
        public int Width { get; set; }

        [DisplayName("(px) طول تصویر")]
        [Range(50, 2000, ErrorMessage = "{0} می بایست در بازه 2000-50 تعیین شود ")]
        public int Height { get; set; }

        [DisplayName("زاویه تصویر")]
        public int Angle { get; set; }
    }
}

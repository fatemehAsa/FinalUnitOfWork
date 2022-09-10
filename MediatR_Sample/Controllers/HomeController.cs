using DocumentFormat.OpenXml.Drawing;
using Infrastructure.DTOs;
using MediatR_Sample.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Models;
using Services.Categories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace MediatR_Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var list = await _categoryService.GetAllCategories(cancellationToken);
            return View(list);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Edit(int id, CancellationToken cancellation)
        {
            var cat = await _categoryService.FindCategoryById(id, cancellation);
            return View(cat);
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategoryOnPost(CancellationToken cancellationToken)
        {

            ErrorClass err = new ErrorClass();
            var data = HttpContext.Request.Form;
            var dic = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            if (dic.FirstOrDefault().Value == "" || dic.LastOrDefault().Value == "")
            {
                err.ErrorId = -1;
                return Json(new { Error = err });
            }
            var category = new Category();
            foreach (var kvp in data.Keys)
            {
                PropertyInfo pi = category.GetType().GetProperty(kvp, BindingFlags.Public | BindingFlags.Instance);
                if (pi != null)
                {
                    pi.SetValue(category, dic[kvp], null);
                }
            }

            if (data.Files.Count > 0)
            {

                IFormFile img = data.Files[0];
                if (ImageValidator.IsImage(img))
                {
                    category.ImageName = NameGenerator.GenerateUniqCode() + System.IO.Path.GetExtension(img.FileName); ;
                    string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/img", category.ImageName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        img.CopyTo(fileStream);
                    }

                    var thumbPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/thumbs", category.ImageName);

                    Utilities.ImageResizer resizer = new Utilities.ImageResizer();
                    resizer.Image_resize(filePath, thumbPath, 150, 150);
                }
                else
                {
                    err.ErrorId = -2;
                    return Json(new { Error = err });
                }

                var success = await _categoryService.AddCategory(category, cancellationToken);
                if (success != null)
                {
                    err.ErrorId = 0;
                }

                return Json(new { Error = err });

            }

            err.ErrorId = -3;
            err.ErrorText = "خطایی رخ داده";
            return Json(new { Error = err });
        }



        [HttpPost]
        public async Task<IActionResult> Edit(Category category, CancellationToken cancellationToken)
        {
            await _categoryService.UpdateCategory(category, cancellationToken);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            bool isExistCatInPost = await _categoryService.IsExistCategoryInPost(id, cancellationToken);
            ViewData["text"] = false;
            if (isExistCatInPost)
            {
                ViewData["text"] = true;
            }
            var cat = await _categoryService.FindCategoryById(id, cancellationToken);
            return View(cat);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Category category, CancellationToken cancellationToken)
        {
            int catId = category.GenreId;
            var result = await _categoryService.RemoveCategory(catId, cancellationToken);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditImage(int cateId, CancellationToken cancellationToken)
        {
            bool hasAvatar = false;
            var cat = await _categoryService.FindCategoryById(cateId, cancellationToken);
            if (cat.ImageName != null)
            {
                hasAvatar = true;
                var model = await _categoryService.GetCategoryForEditPic(cateId, cancellationToken);
                ViewData["HasAvatar"] = hasAvatar;
                return View(model);
            }
            ViewData["HasAvatar"] = hasAvatar;
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> EditImage(PicCategoryViewModel picModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return View(picModel);
            }

            var cat = await _categoryService.FindCategoryById(picModel.Id, cancellationToken);
            var imgName = cat.ImageName;
            if (picModel.AvatarName != null)
            {
                byte[] imageByte;
                var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot/images/img/{imgName}");
                Image original = Image.FromFile(path);
                Image changed = ChangeImage(original, picModel.Width, picModel.Height,picModel.Angle);

                using MemoryStream m = new MemoryStream();
              
                changed.Save(m, ImageFormat.Jpeg);
                imageByte = m.ToArray();

                changed.Dispose();
                original.Dispose();
                System.IO.File.WriteAllBytes(path, imageByte);
                return RedirectToAction("Index");

            }
            else
            {
                ViewData["HasAvatar"] = false;
                return View();
            }


        }

        
        private static Image ChangeImage(Image img ,int width,int height, float angle)
        {
            Bitmap bmp = new Bitmap(img, width, height);
            switch (angle)
            {
                case 0:
                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                    break;
                case 90:
                    bmp.RotateFlip(RotateFlipType.Rotate270FlipXY);
                    break;
                case 180:
                    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case -90:
                    bmp.RotateFlip(RotateFlipType.Rotate90FlipXY);
                    break;
                default:
                    break;
            }
            return bmp;
        }

        public class ErrorClass
        {
            public int ErrorId { get; set; }
            public string ErrorText { get; set; }
        }
    }
}

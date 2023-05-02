﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRAS_Task.Data;
using PRAS_Task.Data.Models;


namespace PRAS_Task.Controllers
{
    public class NewsController : Controller
    {
        DataContext _context;
        IWebHostEnvironment _environment;
        public NewsController(DataContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        [HttpGet]
        [Authorize]
        public ActionResult Index()
        {
            var news = _context.News.ToList();
            return View("Index", news);
        }
        [HttpPost]
        public async Task<ActionResult> Create(New _new) 
        {

            var uploadPath = $"{_environment.WebRootPath}/images";
            var files = Request.Form.Files;
            string filePath;
            if (files[0] != null)
            {
                filePath = @$"{uploadPath}/{files[0].FileName}";
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await files[0].CopyToAsync(fileStream);

                }
                _new.picture = files[0].FileName;
                _new.date = DateTime.Now;
                _context.News.Add(_new);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return BadRequest("Error");

        }
        [ValidateAntiForgeryToken]
        [HttpGet]
        public async Task<ActionResult> CreateView()
        {
            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int Newid)
        {

            var data = _context.News.Where(x => x.Id == Newid).FirstOrDefault();
            if (data != null)
            {
                _context.News.Remove(data);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
                return BadRequest("Cant delete");
    }
        [ValidateAntiForgeryToken]
        public ActionResult UpdateView(int id)
        {
            var _new = _context.News.Where(x => x.Id == id).FirstOrDefault();
            if (_new != null)
            {
                return View("Update", _new);
            }
            return BadRequest("Not exist");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(New model)
        {
            var data = _context.News.Where(x => x.Id == model.Id).FirstOrDefault();

            if (data != null)
            {
                var files = Request.Form.Files;
                string filePath = "";
                string oldPath;
                if (files.Count>0)
                {
                    oldPath = model.picture;

                    var uploadPath = Path.Combine($"{_environment.WebRootPath}", "images");
                    filePath = $"{uploadPath}/{files[0].FileName}";
                    if (oldPath != filePath)
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await files[0].CopyToAsync(fileStream);
                        }
                        model.picture = files[0].FileName;
                        data.picture = model.picture;
                    }

                }
                
                data.text = model.text;
                data.name = model.name;
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
                return BadRequest("Error during processing data");
        }
    }


}

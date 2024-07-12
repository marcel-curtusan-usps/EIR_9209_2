using EIR_9209_2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Image = System.Drawing.Image;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EIR_9209_2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackgroundImageController(ILogger<BackgroundImageController> logger, IInMemoryBackgroundImageRepository backgroundImage) : ControllerBase
    {
        private readonly ILogger<BackgroundImageController> _logger = logger;
        private readonly IInMemoryBackgroundImageRepository _backgroundImage = backgroundImage;
        // GET: api/<BackgroundImage>

        /// <summary>
        /// Get All Background Images.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllImages")]
        public async Task<object> Get()
        {
            //handle bad requests
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_backgroundImage.GetAll());
        }
        /// <summary>
        /// Upload Background Image.
        /// </summary>
        /// <name>UploadBackgroundImage</name>
        /// <param name="file"></param>
        /// <param name="metersPerPixelY"></param>
        /// <param name="metersPerPixelX"></param>
        /// <remarks>
        /// value = "0.0529166667" > 0.01 Pixel Per Meter
        /// value = "0.0002645833" > 0.1 Pixel Per Meter
        /// value = "0.0264583333" > 1 Pixel Per Meter
        /// value = "0.0529166667" > 2 Pixel Per Meter
        /// value = "0.079375" > 3 Pixel Per Meter
        /// value = "0.1322916667" > 5 Pixel Per Meter
        /// value = "0.2645833333" > 10 Pixel Per Meter
        /// value = "0.5291666667" > 20 Pixel Per Meter
        /// value = "1.3229166667" > 50 Pixel Per Meter
        /// value = "2.6458333333" > 100 Pixel Per Meter
        /// value = "26.4583333333" > 1000 Pixel Per Meter
        /// </remarks>
        /// <returns>Background Image  has been Loaded</returns>
        /// <response code="201">Returns When Background Image has been Loaded</response>
        /// <response code="400">If the File name was provided </response>
        [HttpPost]
        [Route("UploadBackgroundImage")]
        public async Task<IActionResult> UploadBackgroundImage(IFormFile file, double metersPerPixelY, double metersPerPixelX)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }
                // convert the file to a byte array to image
                byte[] fileBytes;
                string fileName = file.FileName;
                BackgroundImage newImage = new();
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    fileBytes = ms.ToArray();
                    string imageBase64Data = Convert.ToBase64String(fileBytes);
                    using (Image image = Image.FromStream(ms))
                    {
                        // You can perform various operations on the image here
                        // For example, you can resize, crop, or apply filters to the image
                        // Once you're done with the image, you can save it or use it as needed
                        newImage.origoX = image.Width;
                        newImage.origoY = image.Height;

                        newImage.base64 = string.Concat("data:image/png;base64,", imageBase64Data);
                        newImage.fileName = fileName;
                        newImage.id = Guid.NewGuid().ToString();
                        newImage.widthMeter = newImage.origoX * metersPerPixelY;
                        newImage.heightMeter = newImage.origoY * metersPerPixelX;
                        newImage.metersPerPixelY = metersPerPixelY;
                        newImage.metersPerPixelX = metersPerPixelX;


                        //sent to the repository
                        await _backgroundImage.LoadBackgroundImage(newImage);
                    }

                }


                return Ok(new JObject { ["message"] = "Image was uploaded successfully." });
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e.Message);
            }

        }


    }

}

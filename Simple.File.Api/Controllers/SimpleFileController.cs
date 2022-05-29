using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Simple.File.Api.Interfaces;
using Simple.File.Api.Utilities;

namespace Simple.File.Api.Controllers
{
    [Authorize]  
    [Route("api/[controller]")]
    [ApiController]
    public class SimpleFileController : ControllerBase
    {
        private readonly IUserFileStorage _fileStorage;
        private readonly SimpleFileStorageOptions _storageOptions;

        public SimpleFileController(
            IUserFileStorage storage,
            SimpleFileStorageOptions options)
        {
            _fileStorage = storage;
            _storageOptions = options;
        }

        // GET: api/SimpleFile/sha256?filename=xxx
        [HttpGet()]
        [Route("sha256")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<string> CalculateSha256([FromQuery] string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return BadRequest("No filename has been provided.");
            if (!_fileStorage.Exists(filename))
                return NotFound();
            return _fileStorage.CalculateSha256(filename);
        }

        // POST: api/SimpleFile/upload
        [HttpPost]
        [Route("upload")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    
        public async Task<IActionResult> Upload(IFormFile formFile)
        {
            // Request must be form data
            if (!Request.HasFormContentType)
                return BadRequest("Form content type required.");

            // Boundary must be found in request's content type
            var boundary = Request.GetMultipartBoundary();
            if (string.IsNullOrWhiteSpace(boundary))
                return new UnsupportedMediaTypeResult();

            if (formFile.Length > _storageOptions.FileSizeLimitInBytes)
                return BadRequest($"File size limit of {_storageOptions.FileSizeLimitInBytes} bytes reached.");
                
            if (!ContentDispositionHeaderValue.TryParse(formFile.ContentDisposition, out var header))
            {
                return BadRequest("No content disposition header");
            }
            
            if (header != null && FileHelper.HasFileContentDisposition(header))
            {
                using (var streamToSave = formFile.OpenReadStream())
                {
                    await _fileStorage.Store(streamToSave, formFile.FileName);
                    return Ok();
                }
            }

            return BadRequest("No file content disposition header");
        }
        

        // DELETE: api/SimpleFile?filename=xxx
        [HttpDelete()]
        public IActionResult Delete([FromQuery] string filename)
        {
            if (string.IsNullOrEmpty(filename))
                return BadRequest("No filename has been provided.");
            if (!_fileStorage.Exists(filename))
                return NotFound();
            
            _fileStorage.Delete(filename);
            return Ok();
        }
    }
}

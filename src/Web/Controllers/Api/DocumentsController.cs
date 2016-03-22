﻿using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Routing;
using Web.ViewModels.Api.Documents;

namespace Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DocumentsController : BaseController
    {
        private readonly IMediator _mediator;

        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:int}/view")]
        public async Task<IActionResult> View(View.Query query)
        {
            var model = await _mediator.SendAsync(query);
            return File(model.FileContents, model.ContentType);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(Get.Query query)
        {
            var model = await _mediator.SendAsync(query);

            return Ok(model);
        }

        [HttpGet("{id:int}/thumbnail")]
        public async Task<IActionResult> Thumbnail(Thumbnail.Query query)
        {
            var model = await _mediator.SendAsync(query);

            return File(model.FileContents, model.ContentType);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Post.Command command)
        {
            var documentId = await _mediator.SendAsync(command);

            if (documentId == null)
            {
                return new HttpStatusCodeResult((int) HttpStatusCode.InternalServerError);
            }

            return CreatedAtAction(nameof(ViewModels.Api.Documents.Get)
                , new RouteValueDictionary(new Get.Query {Id = documentId}), null);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(Put.Command command)
        {
            var documentId = await _mediator.SendAsync(command);

            return await Get(new Get.Query {Id = documentId});
        }
    }
}
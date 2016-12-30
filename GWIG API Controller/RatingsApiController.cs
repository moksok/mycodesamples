using Sabio.Web.Domain;
using Sabio.Web.Models.Requests.Rating;
using Sabio.Web.Models.Responses;
using Sabio.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Practices.Unity;
using Sabio.Web.Enums;
using Sabio.Web.Models.SystemEvents;
using Sabio.Web.Services.Interface;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/rating")]
    public class RatingsApiController : ApiController
    {
        [Dependency]
        public ISystemEventsService _SystemEventsService { get; set; }

        [Dependency]
        public IRatingService _RatingService { get; set; }

        [Route(), HttpPost]
        public HttpResponseMessage postReviewAndRating(RatingRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            model.userId = UserService.GetCurrentUserId();

            int ratingInt = _RatingService.RatingAndReviewPost(model);
            ItemResponse<int> newitemResponse = new ItemResponse<int>();
            newitemResponse.Item = ratingInt;

            _SystemEventsService.AddSystemEvent(new AddSystemEventModel
            {
                ActorUserId = UserService.GetCurrentUserId(),
                ActorType = ActorType.User,
                EventType = SystemEventType.UserPlaceReview,
                TargetId = model.placesId,
                TargetType = TargetType.Place
            });

            return Request.CreateResponse(HttpStatusCode.OK, newitemResponse);
        }

        [Route("{placesId:int}"), HttpGet]
        public HttpResponseMessage getReviewAndRatingByplacesId(int placesId)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            List<RatingDomain> Rating = _RatingService.RatingAndReviewGetByplacesId(placesId);

            ItemsResponse<RatingDomain> Response = new ItemsResponse<RatingDomain>();

            Response.Items = Rating;


            return Request.CreateResponse(HttpStatusCode.OK, Response.Items);
        }

        [Route(), HttpGet]
        public HttpResponseMessage getAllRatingsAndReview()
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            List<RatingDomain> RatingAndReview = _RatingService.RatingAndReviewGetAll();

            ItemsResponse<RatingDomain> Response = new ItemsResponse<RatingDomain>();
            Response.Items = RatingAndReview;

            return Request.CreateResponse(HttpStatusCode.OK, Response.Items);
        }

        [Route("{placesId:int}"), HttpPut]
        public HttpResponseMessage updateRatingsAndReviewByuserIdandplacesId(RatingRequest model, int placesId)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _RatingService.RatingsAndReviewUpdateByuserIdAndplacesIdAndId(model, placesId);

            ItemResponse<bool> Response = new ItemResponse<bool>();
            Response.IsSuccessful = true;
            return Request.CreateResponse(HttpStatusCode.OK, Response);
        }

        [Route("{placesId:int}/avg"), HttpGet]
        public HttpResponseMessage getAvgRatingsByplacesId(int placesId)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            RatingDomain avgRating = _RatingService.avgRatingGet(placesId);
            ItemResponse<decimal> Response = new ItemResponse<decimal>();
            Response.Item = avgRating.Rating;
            return Request.CreateResponse(HttpStatusCode.OK, Response.Item);
        }
        [Route("{Id:int}"), HttpDelete]
        public HttpResponseMessage deleteReview(int Id)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            _RatingService.RatingAndReviewDelete(Id);

            ItemResponse<bool> Response = new ItemResponse<bool>();
            Response.IsSuccessful = true;

            return Request.CreateResponse(HttpStatusCode.OK, Response);

        }
    }
}



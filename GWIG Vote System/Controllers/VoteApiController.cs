using Sabio.Web.Domain;
using Sabio.Web.Models.Vote;
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
    [RoutePrefix("api/vote")]
    public class VoteApiController : ApiController
    {
        [Dependency]
        public ISystemEventsService SystemEventService { get; set; }

        [Dependency]
        public IRatingService _RatingService { get; set; }

        [Dependency]
        public IUserProfileService _userProfileService { get; set; }

        [Route(), HttpPost]
        [Authorize]
        public HttpResponseMessage InsertVote(InsertVoteRequest model)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            //get current logged-in user
            model.VoterId = UserService.GetCurrentUserId();

            //insert new row into dbo.PointScore, this creates a log for each vote casted
            VoteService.VoteInsert(model);

            //updates overall point score for each username
            _userProfileService.UpdateUserPointScore(model.UserName, model.NetVote);

            //updates the review's individual point score
            if (model.isMedia)
            {
                _RatingService.UpdateReviewPointScoreForMedia(model.ContentId, model.NetVote);
            }
            else
            {
                _RatingService.UpdateReviewPointScore(model.ContentId, model.NetVote);
            }
            SystemEventService.AddSystemEvent(new AddSystemEventModel
            {
                ActorUserId = model.VoterId,
                ActorType = ActorType.User,
                EventType = SystemEventType.UserVote,
                TargetId = model.ContentId,
                TargetType = TargetType.Review
            });

            return Request.CreateResponse(HttpStatusCode.OK, model);
        }
    }
}

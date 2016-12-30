using Sabio.Web.Models.Requests.Rating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using Sabio.Data;
using Sabio.Web.Domain;
using Sabio.Web.Services.Interface;
using Sabio.Web.Enums;

namespace Sabio.Web.Services
{
    public class RatingService : BaseService, IRatingService
    {
        //POST
        public int RatingAndReviewPost(RatingRequest model)
        {
            int id = 0;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Rating_Insert"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@placesId", model.placesId);
                     paramCollection.AddWithValue("@userId", model.userId);
                     paramCollection.AddWithValue("@Rating", model.Rating);
                     paramCollection.AddWithValue("@Subject", model.Subject);
                     paramCollection.AddWithValue("@Review", model.Review);
                     paramCollection.AddWithValue("@UserName", model.userName);

                    SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                    p.Direction = System.Data.ParameterDirection.Output;

                    paramCollection.Add(p);
                }, returnParameters: delegate (SqlParameterCollection param)
                {
                    id = (int)param["@Id"].Value;
                }
                );
            return id;
        }

        //Get
        public List<RatingDomain> RatingAndReviewGetByplacesId(int placesId)
        {
            List<RatingDomain> ListRatingDomain = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Rating_SelectByPlacesId"
                , inputParamMapper: delegate (SqlParameterCollection paramcollection)
                {
                    paramcollection.AddWithValue("@placesId", placesId);
                }, map: delegate (IDataReader reader, short set)
                {
                    RatingDomain p = new RatingDomain();
                    int startingIndex = 0;
                    p.Id = reader.GetSafeInt32(startingIndex++);
                    p.placesId = reader.GetSafeInt32(startingIndex++);
                    p.userId = reader.GetSafeString(startingIndex++);
                    p.Rating = reader.GetSafeDecimal(startingIndex++);
                    p.Subject = reader.GetSafeString(startingIndex++);
                    p.Review = reader.GetSafeString(startingIndex++);
                    p.Created = reader.GetSafeDateTime(startingIndex++);
                    p.reviewPointScore = reader.GetSafeInt32(startingIndex++);
                    p.userName = reader.GetSafeString(startingIndex++);


                    if (ListRatingDomain == null)
                    {
                        ListRatingDomain = new List<RatingDomain>();
                    }
                    ListRatingDomain.Add(p);
                });

            return Decorate(ListRatingDomain);
        }

        //Decorator Pattern
        //Filters reviews to return true or false if current logged-in user has voted on specific review
        private List<RatingDomain> Decorate(List<RatingDomain> ReviewsByPlaceId)
        {

            //insert current logged in user id and new list of review id's as parameters for SP          
            if (UserService.IsLoggedIn() && ReviewsByPlaceId != null)
            {

                //Create two lists for comparison
                List<ReviewsByPlaces> ReviewsVotedList = new List<ReviewsByPlaces>();

                List<int> ReviewIdList = new List<int>();

                //extract review id from each review in another list
                foreach (RatingDomain Review in ReviewsByPlaceId)
                {
                    ReviewIdList.Add(Review.Id);
                }

                string userId = UserService.GetCurrentUserId();

                //insert all review id's for specific place and returns a list for all reviews that
                //logged in user has voted on
                DataProvider.ExecuteCmd(GetConnection, "dbo.PointScore_GetByUserIdAndContentId"
                    , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                    {
                        paramCollection.AddWithValue("@VoterId", userId);

                        SqlParameter s = new SqlParameter("@ContentId", SqlDbType.Structured);
                        if (ReviewIdList != null && ReviewIdList.Any())
                        {
                            s.Value = new IntIdTable(ReviewIdList);
                        }
                        paramCollection.Add(s);

                    }, map: delegate (IDataReader reader, short set)
                    {
                        ReviewsByPlaces lol = new ReviewsByPlaces();
                        int startingIndex = 0;
                        lol.NetVote = reader.GetSafeInt32(startingIndex++);                        
                        lol.ContentId = reader.GetSafeInt32(startingIndex++);
                        lol.ReviewId = reader.GetSafeInt32(startingIndex++);
                        lol.VoterId = reader.GetSafeString(startingIndex++);                      
                        ReviewsVotedList.Add(lol);
                    }
                );


                //loop through both lists and set hasVoted to true if id's match
                if (ReviewsVotedList != null)
                {
                    foreach (RatingDomain Review in ReviewsByPlaceId)
                    {
                        foreach (ReviewsByPlaces ReviewByPlace in ReviewsVotedList)
                        {
                            if (ReviewByPlace.ContentId == Review.Id)
                            {
                                Review.hasVoted = true;
                                Review.NetVote = ReviewByPlace.NetVote;
                            }
                        }
                    }
                }

                //return list with tailored hasVoted field
                return ReviewsByPlaceId;

            }
            else
            {
                return ReviewsByPlaceId;
            }

        }


        //Getall
        public List<RatingDomain> RatingAndReviewGetAll()
        {
            List<RatingDomain> ratingsList = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Rating_SelectAll"
                , inputParamMapper: null
                , map: delegate (IDataReader reader, short set)
                {
                    RatingDomain p = new RatingDomain();
                    int startingIndex = 0;
                    p.Id = reader.GetSafeInt32(startingIndex++);
                    p.placesId = reader.GetSafeInt32(startingIndex++);
                    p.userId = reader.GetSafeString(startingIndex++);
                    p.Rating = reader.GetSafeDecimal(startingIndex++);
                    p.Subject = reader.GetSafeString(startingIndex++);
                    p.Review = reader.GetSafeString(startingIndex++);
                    p.Created = reader.GetSafeDateTime(startingIndex++);
                    p.reviewPointScore = reader.GetSafeInt32(startingIndex++);

                    if (ratingsList == null)
                    {
                        ratingsList = new List<RatingDomain>();
                    }
                    ratingsList.Add(p);
                }
                );
            return ratingsList;
        }

        //Update
        public void RatingsAndReviewUpdateByuserIdAndplacesIdAndId(RatingRequest model, int placesId)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Rating_UpdateByPlacesById"
                , inputParamMapper: delegate (SqlParameterCollection param)
                {
                    param.AddWithValue("@Id", model.Id);
                    param.AddWithValue("@placesId", placesId);
                    param.AddWithValue("@userId", model.userId);
                    param.AddWithValue("@Rating", model.Rating);
                    param.AddWithValue("@Subject", model.Subject);
                    param.AddWithValue("@Review", model.Review);
                }
                );

        }

        public RatingDomain avgRatingGet(int placesId)
        {
            RatingDomain avgRating = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.Rating_AvgRating"
                , inputParamMapper: delegate (SqlParameterCollection param)
                {
                    param.AddWithValue("@placesId", placesId);
                }
                , map: delegate (IDataReader reader, short set)
                {
                    avgRating = new RatingDomain();
                    int startingIndex = 0;
                    avgRating.Rating = reader.GetSafeDecimal(startingIndex++);
                }
                );
            return avgRating;
        }

        public void RatingAndReviewDelete(int id)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Rating_DeleteById"
                , inputParamMapper: delegate (SqlParameterCollection param)
                {
                    param.AddWithValue("@Id", id);
                }
                );
        }

        //update individual reviews' vote score in dbo.ratings
        public void UpdateReviewPointScore(int id, int netVote)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Ratings_UpdateReviewPointScore"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {

                   paramCollection.AddWithValue("@Id", id);
                   paramCollection.AddWithValue("@NetVote", netVote);
               }

            );
        }

        public void UpdateReviewPointScoreForMedia(int id, int netVote)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.Ratings_UpdateReviewPointScoreForMedia"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", id);
                   paramCollection.AddWithValue("@NetVote", netVote);
               }

            );
        }

        public int PostPlacesRatingInsert(PlacesRatingsRequest model)
        {
            int Id = 0;
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.PlacesRatings_Insert"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@PlacesId", model.PlaceId);
                     paramCollection.AddWithValue("@RatingType", model.RatingType);
                     paramCollection.AddWithValue("@Rating", model.Rating);
                     paramCollection.AddWithValue("@UserId", model.UserId);
                     paramCollection.AddWithValue("@GroupId", model.GroupId);
                     paramCollection.AddWithValue("@AspectId", model.AspectId);

                     SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                     p.Direction = System.Data.ParameterDirection.Output;

                     paramCollection.Add(p);
                 }, returnParameters: delegate (SqlParameterCollection param)
                 {
                     Id = (int)param["@Id"].Value;
                 }

                );
                return Id;
        }

        public List<PlacesRatingDomain> GetPlacesRatingSelectByplaceId(int placeId)
        {
            List<PlacesRatingDomain> Places = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.PlacesRatings_SelectByPlacesId"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                 {
                     paramCollection.AddWithValue("@PlaceId",placeId);
 
                 }
                 , map: delegate (IDataReader reader, short set)
                 {
                     PlacesRatingDomain Place = new PlacesRatingDomain();
                     int startingIndex = 0;
                     Place.Id = reader.GetSafeInt32(startingIndex++);
                     Place.Created = reader.GetSafeDateTime(startingIndex++);
                     Place.PlaceId = reader.GetSafeInt32(startingIndex++);
                     Place.RatingType = reader.GetSafeEnum<RatingType>(startingIndex++);
                     Place.Rating = reader.GetSafeDecimal(startingIndex++);
                     Place.UserId = reader.GetSafeString(startingIndex++);
                     Place.GroupId = reader.GetSafeInt32(startingIndex++);
                     Place.AspectId = reader.GetSafeInt32(startingIndex++);

                     if (Places == null)
                     {
                         Places = new List<PlacesRatingDomain>();
                     }
                     Places.Add(Place);
                 }
                );
            return Places;
        }

        public RatingDomain GetRatingDomainRatingId(int ratingId)
        {
            RatingDomain Review = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Ratings_GetPlaceIdByRatingId"

                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@ratingId", ratingId);
                }
                , map: delegate (IDataReader reader, short set)
                {
                    Review = new RatingDomain();
                    int startingIndex = 0; //startingOrdinal

                    Review.Id = reader.GetSafeInt32(startingIndex++);
                    Review.placesId = reader.GetSafeInt32(startingIndex++);
                    Review.userId = reader.GetSafeString(startingIndex++);
                    Review.Rating = reader.GetSafeDecimal(startingIndex++);
                    Review.Subject = reader.GetSafeString(startingIndex++);
                    Review.Review = reader.GetSafeString(startingIndex++);
                    Review.Created = reader.GetDateTime(startingIndex++);
                    Review.reviewPointScore = reader.GetSafeInt32(startingIndex++);
                    Review.userName = reader.GetSafeString(startingIndex++);

                }
            );

            return Review;
        }

    }
}
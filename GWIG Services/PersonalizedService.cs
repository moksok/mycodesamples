using Sabio.Data;
using Sabio.Web.Domain;
using Sabio.Web.Enums;
using Sabio.Web.Models.Requests.PersonalizedFriends;
using Sabio.Web.Services.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Sabio.Web.Services
{
    public class PersonalizedService : BaseService, IPersonalizedService
    {
        public List<PersonalizedFriendsDomain> GetFriendsOfFriendsReviews(PersonalizedFriendRequest model, string userId)
        {
            List<PersonalizedFriendsDomain> ListPersonalizedDomain = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.Followers_SelectFriendsOfFriendsReviewedPlacesWithProfile"
                , inputParamMapper: delegate (SqlParameterCollection paramcollection)
                {
                    paramcollection.AddWithValue("@UserId", userId);
                    paramcollection.AddWithValue("@FriendRecursionLevel", model.FriendRecursionLevel);
                    paramcollection.AddWithValue("@CurrentPage", model.CurrentPage);
                    paramcollection.AddWithValue("@ItemsPerPage", model.ItemsPerPage);
                    paramcollection.AddWithValue("@UserSelected", model.selectedId);

                }, map: delegate (IDataReader reader, short set)
                {
                    PersonalizedFriendsDomain p = new PersonalizedFriendsDomain();
                    int startingIndex = 0;
                    p.placesId= reader.GetSafeInt32(startingIndex++);
                    p.Name = reader.GetSafeString(startingIndex++);
                    p.Description = reader.GetSafeString(startingIndex++);
                    p.PhoneNumber = reader.GetSafeString(startingIndex++);
                    p.Website = reader.GetSafeString(startingIndex++);
                    p.MediaId = reader.GetSafeInt32(startingIndex++);                    
                    p.LocationId = reader.GetSafeInt32(startingIndex++);
                    p.CategoryId = reader.GetSafeInt32(startingIndex++);
                    p.Slug = reader.GetSafeString(startingIndex++);
                    p.CityId = reader.GetSafeInt32(startingIndex++);
                    p.Price = reader.GetSafeInt32(startingIndex++);
                    p.UserId = reader.GetSafeString(startingIndex++);
                    p.FriendLevel = reader.GetSafeInt32(startingIndex++);
                    p.Url = reader.GetSafeString(startingIndex++);
                    p.Address1 = reader.GetSafeString(startingIndex++);
                    p.City = reader.GetSafeString(startingIndex++);
                    p.State = reader.GetSafeString(startingIndex++);
                    p.ZipCode = reader.GetSafeString(startingIndex++);
                    p.Latitude = reader.GetSafeDecimal(startingIndex++);
                    p.Longitude = reader.GetSafeDecimal(startingIndex++);
                    p.userName = reader.GetSafeString(startingIndex++);
                    p.profileMediaId = reader.GetSafeInt32(startingIndex++);
                    p.profileUrl = reader.GetSafeString(startingIndex++);

                    if (ListPersonalizedDomain == null)
                    {
                        ListPersonalizedDomain = new List<PersonalizedFriendsDomain>();
                    }
                    ListPersonalizedDomain.Add(p);
                });

            return ListPersonalizedDomain;
        }

        public List<PersonalizedRatingDomain> GetPersonalReviews(string userId)
        {
            List<PersonalizedRatingDomain> ListPersonalizedRatingDomain = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.RatingAndExtPlaceForGoogleMarkers_SelectByUserId"
                , inputParamMapper: delegate (SqlParameterCollection paramcollection)
                {
                    paramcollection.AddWithValue("@userId", userId);

                }, map: delegate (IDataReader reader, short set)
                {
                    PersonalizedRatingDomain p = new PersonalizedRatingDomain();
                    int startingIndex = 0;

                    p.Id = reader.GetSafeInt32(startingIndex++);
                    p.Created = reader.GetDateTime(startingIndex++);
                    p.placesId = reader.GetSafeInt32(startingIndex++);
                    p.UserId = reader.GetSafeString(startingIndex++);
                    p.Rating = reader.GetSafeDecimal(startingIndex++);
                    p.Subject = reader.GetSafeString(startingIndex++);
                    p.Review = reader.GetSafeString(startingIndex++);
                    p.reviewPointScore = reader.GetSafeInt32(startingIndex++);
                    p.userName = reader.GetSafeString(startingIndex++);
                    p.Name = reader.GetSafeString(startingIndex++);
                    p.Description = reader.GetSafeString(startingIndex++);
                    p.PhoneNumber = reader.GetSafeString(startingIndex++);
                    p.Website = reader.GetSafeString(startingIndex++);
                    p.MediaId = reader.GetSafeInt32(startingIndex++);
                    p.LocationId = reader.GetSafeInt32(startingIndex++);
                    p.CategoryId = reader.GetSafeInt32(startingIndex++);
                    p.Slug = reader.GetSafeString(startingIndex++);
                    p.CityId = reader.GetSafeInt32(startingIndex++);
                    p.Price = reader.GetSafeInt32(startingIndex++);                    
                    p.Url = reader.GetSafeString(startingIndex++);
                    p.Address1 = reader.GetSafeString(startingIndex++);
                    p.City = reader.GetSafeString(startingIndex++);
                    p.State = reader.GetSafeString(startingIndex++);
                    p.ZipCode = reader.GetSafeString(startingIndex++);
                    p.Latitude = reader.GetSafeDecimal(startingIndex++);
                    p.Longitude = reader.GetSafeDecimal(startingIndex++);
                    

                    if (ListPersonalizedRatingDomain == null)
                    {
                        ListPersonalizedRatingDomain = new List<PersonalizedRatingDomain>();
                    }
                    ListPersonalizedRatingDomain.Add(p);
                });

            return ListPersonalizedRatingDomain;
        }

        public List<PersonalizedFavoritePlaceDomain> GetFavoritePlaces(string userId, int favoriteType)
        {
            List<PersonalizedFavoritePlaceDomain> ListPersonalizedFavoritePlaceDomain = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.UserFavoritePlaces_SelectByUserIdAndFavoriteType"
                , inputParamMapper: delegate (SqlParameterCollection paramcollection)
                {
                    paramcollection.AddWithValue("@userId", userId);
                    paramcollection.AddWithValue("@FavoriteType", favoriteType);

                }, map: delegate (IDataReader reader, short set)
                {
                    PersonalizedFavoritePlaceDomain p = new PersonalizedFavoritePlaceDomain();
                    int startingIndex = 0;

                    p.Id = reader.GetSafeInt32(startingIndex++);
                    p.UserId = reader.GetSafeString(startingIndex++);
                    p.PlacesId = reader.GetSafeInt32(startingIndex++);
                    p.FavoriteType = reader.GetSafeEnum<UserFavoritePlacesType>(startingIndex++);
                    p.PointScore = reader.GetSafeInt32(startingIndex++);
                    p.Name = reader.GetSafeString(startingIndex++);
                    p.Description = reader.GetSafeString(startingIndex++);
                    p.PhoneNumber = reader.GetSafeString(startingIndex++);
                    p.Website = reader.GetSafeString(startingIndex++);
                    p.MediaId = reader.GetSafeInt32(startingIndex++);
                    p.LocationId = reader.GetSafeInt32(startingIndex++);
                    p.CategoryId = reader.GetSafeInt32(startingIndex++);
                    p.Slug = reader.GetSafeString(startingIndex++);
                    p.CityId = reader.GetSafeInt32(startingIndex++);
                    p.Price = reader.GetSafeInt32(startingIndex++);
                    p.Url = reader.GetSafeString(startingIndex++);
                    p.Address1 = reader.GetSafeString(startingIndex++);
                    p.City = reader.GetSafeString(startingIndex++);
                    p.State = reader.GetSafeString(startingIndex++);
                    p.ZipCode = reader.GetSafeString(startingIndex++);
                    p.Latitude = reader.GetSafeDecimal(startingIndex++);
                    p.Longitude = reader.GetSafeDecimal(startingIndex++);


                    if (ListPersonalizedFavoritePlaceDomain == null)
                    {
                        ListPersonalizedFavoritePlaceDomain = new List<PersonalizedFavoritePlaceDomain>();
                    }
                    ListPersonalizedFavoritePlaceDomain.Add(p);
                });

            return ListPersonalizedFavoritePlaceDomain;
        }

        public List<PersonalizedFollowingPlaceDomain> GetFollowingPlaces(string userId)
        {
            List<PersonalizedFollowingPlaceDomain> ListPersonalizedFollowingPlaceDomain = null;

            DataProvider.ExecuteCmd(GetConnection, "FollowingPlacesAndExtPlaceForGoogleMarkers_SelectByUserId"
                , inputParamMapper: delegate (SqlParameterCollection paramcollection)
                {
                    paramcollection.AddWithValue("@UserId", userId);

                }, map: delegate (IDataReader reader, short set)
                {
                    PersonalizedFollowingPlaceDomain p = new PersonalizedFollowingPlaceDomain();
                    int startingIndex = 0;

                    p.Id = reader.GetSafeInt32(startingIndex++);
                    p.Created = reader.GetSafeDateTime(startingIndex++);
                    p.PlacesId = reader.GetSafeInt32(startingIndex++);
                    p.UserId = reader.GetSafeString(startingIndex++);                    
                    p.Name = reader.GetSafeString(startingIndex++);
                    p.Description = reader.GetSafeString(startingIndex++);
                    p.PhoneNumber = reader.GetSafeString(startingIndex++);
                    p.Website = reader.GetSafeString(startingIndex++);
                    p.MediaId = reader.GetSafeInt32(startingIndex++);
                    p.LocationId = reader.GetSafeInt32(startingIndex++);
                    p.CategoryId = reader.GetSafeInt32(startingIndex++);
                    p.Slug = reader.GetSafeString(startingIndex++);
                    p.CityId = reader.GetSafeInt32(startingIndex++);
                    p.Price = reader.GetSafeInt32(startingIndex++);
                    p.Url = reader.GetSafeString(startingIndex++);
                    p.Address1 = reader.GetSafeString(startingIndex++);
                    p.City = reader.GetSafeString(startingIndex++);
                    p.State = reader.GetSafeString(startingIndex++);
                    p.ZipCode = reader.GetSafeString(startingIndex++);
                    p.Latitude = reader.GetSafeDecimal(startingIndex++);
                    p.Longitude = reader.GetSafeDecimal(startingIndex++);


                    if (ListPersonalizedFollowingPlaceDomain == null)
                    {
                        ListPersonalizedFollowingPlaceDomain = new List<PersonalizedFollowingPlaceDomain>();
                    }
                    ListPersonalizedFollowingPlaceDomain.Add(p);
                });

            return ListPersonalizedFollowingPlaceDomain;
        }
    }
}
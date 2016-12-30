using Sabio.Web.Models.Requests;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Sabio.Web.Domain;
using Sabio.Data;
using Sabio.Data.Providers;
using System.Data;
using System.Drawing;
using System.IO;
using Sabio.Web.Domain.MyMedia;
using Sabio.Web.Enums;
using Sabio.Web.Models;
using Sabio.Web.Models.Requests.Followers;
using Sabio.Web.Services.Interface;
using Microsoft.Practices.Unity;

namespace Sabio.Web.Services
{
    public class _userProfileService : BaseService, IUserProfileService
    {
        [Dependency]
        public IFollowersService FollowersService { get; set; }

        public object CategoryService { get; private set; }

        public int CreateProfile(string userId, CreateUserProfileJsonData model) // post
        {
            int _id = 0;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UserProfile_InsertNewUser"
              , inputParamMapper: delegate (SqlParameterCollection paramCollection)
              {

                  paramCollection.AddWithValue("@firstName", model.firstName);
                  paramCollection.AddWithValue("@lastName", model.lastName);
                  paramCollection.AddWithValue("@profileContent", model.profileContent);
                  paramCollection.AddWithValue("@tagLine", model.tagLine);
                  paramCollection.AddWithValue("@userName", model.userName);
                  paramCollection.AddWithValue("@userID", userId);
                  SqlParameter p = new SqlParameter("@Id", System.Data.SqlDbType.Int);
                  p.Direction = System.Data.ParameterDirection.Output;

                  paramCollection.Add(p);

              },
               returnParameters: delegate (SqlParameterCollection param)
               {
                   int.TryParse(param["@Id"].Value.ToString(), out _id);
               }
               );
            return _id;
        }

        public List<UserMini> Get()
        {
            List<UserMini> list = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.UserProfile_GetUserProfile"
               , inputParamMapper: null
               , map: delegate (IDataReader reader, short set)
               {
                   UserMini uid = new UserMini();
                   int startingIndex = 0;

                   // REASSIGNED: UserMini Domain Model Id - From Int to String -
                   uid.Id = reader.GetSafeString(startingIndex++);
                   uid.FirstName = reader.GetSafeString(startingIndex++);
                   uid.LastName = reader.GetSafeString(startingIndex++);
                   uid.ProfileContent = reader.GetSafeString(startingIndex++);
                   uid.TagLine = reader.GetSafeString(startingIndex++);
                   uid.UserId = reader.GetSafeString(startingIndex++);

                   if (list == null)
                   {
                       list = new List<UserMini>();
                   }

                   list.Add(uid);
               }
               );


            return list;
        }

        public void Put(UserProfileUpdateRequest model)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UserProfile_updateInformation"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {

                   paramCollection.AddWithValue("@firstName", model.firstName);
                   paramCollection.AddWithValue("@lastName", model.lastName);
                   paramCollection.AddWithValue("@profileContent", model.profileContent);
                   paramCollection.AddWithValue("@tagLine", model.tagLine);
                   paramCollection.AddWithValue("@userId", model.id);

               }

            );
        }

        public void Delete(int id)
        {
            int deleteId = id;

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UserProfile_deleteById"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {
                   paramCollection.AddWithValue("@Id", deleteId);

               });



        }

        //dropzone C# to resize the image to the appropriate avatar size
        public System.Drawing.Image ResizeMyImage(System.Drawing.Image image, int maxHeight)
        {
            var ratio = (double)maxHeight / image.Height;
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);
            var newImage = new Bitmap(newWidth, newHeight);
            using (var g = Graphics.FromImage(newImage))
            {
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string dirFullPath = HttpContext.Current.Server.MapPath("~/MediaUploader/");
            string[] files;
            int numFiles;
            files = System.IO.Directory.GetFiles(dirFullPath);
            numFiles = files.Length;
            numFiles = numFiles + 1;

            string str_image = "";

            foreach (string s in context.Request.Files)
            {
                HttpPostedFile file = context.Request.Files[s];
                string fileName = file.FileName;
                string fileExtension = file.ContentType;

                if (!string.IsNullOrEmpty(fileName))
                {
                    fileExtension = Path.GetExtension(fileName);
                    str_image = "MyPHOTO_" + numFiles.ToString() + fileExtension;
                    string pathToSave = HttpContext.Current.Server.MapPath("~/MediaUploader/") + str_image;
                    System.Drawing.Bitmap bmpPostedImage = new System.Drawing.Bitmap(file.InputStream);

                    //ResizeMyImage method call
                    System.Drawing.Image objImage = ResizeMyImage(bmpPostedImage, 200);
                    objImage.Save(pathToSave, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            context.Response.Write(str_image);
        }

        public PublicUserProfileDomain GetPublicUserById(string userId)
        {
            PublicUserProfileDomain PublicUser = null;
            Media Media = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AspNetUsers_UserProfile_SelectById"

                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@userId", userId);
                }
                , map: delegate (IDataReader reader, short set)
                {
                    PublicUser = new PublicUserProfileDomain();
                    int startingIndex = 0; //startingOrdinal

                    PublicUser._id = reader.GetSafeInt32(startingIndex++);
                    PublicUser.createdDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.followersCount = reader.GetSafeInt32(startingIndex++);
                    PublicUser.followingCount = reader.GetSafeInt32(startingIndex++);
                    PublicUser.firstName = reader.GetSafeString(startingIndex++);
                    PublicUser.lastName = reader.GetSafeString(startingIndex++);
                    PublicUser.profileContent = reader.GetSafeString(startingIndex++);
                    PublicUser.mediaId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.bgmediaId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.pointScore = reader.GetSafeInt32(startingIndex++);
                    PublicUser.locationId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.joinedDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.lastLoggedInDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.tagLine = reader.GetSafeString(startingIndex++);
                    PublicUser.userId = reader.GetSafeString(startingIndex++);
                    PublicUser.Email = reader.GetSafeString(startingIndex++);
                    PublicUser.EmailConfirmed = reader.GetSafeBool(startingIndex++);
                    PublicUser.PhoneNumber = reader.GetSafeString(startingIndex++);
                    PublicUser.UserName = reader.GetSafeString(startingIndex++);

                    Media = new Media();


                    Media.Url = reader.GetSafeString(startingIndex++);
                    Media.Title = reader.GetSafeString(startingIndex++);
                    Media.Description = reader.GetSafeString(startingIndex++);
                    Media.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    Media.DataType = reader.GetSafeString(startingIndex++);
                    Media.Id = reader.GetSafeInt32(startingIndex++);

                    PublicUser.MyMedia = Media;

                    Media = new Media();

                    Media.Url = reader.GetSafeString(startingIndex++);
                    Media.Title = reader.GetSafeString(startingIndex++);
                    Media.Description = reader.GetSafeString(startingIndex++);
                    Media.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    Media.DataType = reader.GetSafeString(startingIndex++);
                    Media.Id = reader.GetSafeInt32(startingIndex++);

                    PublicUser.BgMyMedia = Media;

                    PublicUser.isFollowed = DecorateisFollowing(PublicUser.userId);
                    PublicUser.isFollower = DecorateisFollower(PublicUser.userId);



                }
            );

            return PublicUser;
        }



        //decorate function runs SP on this model
        public bool DecorateisFollowing(string followedUserId)
        {
            bool isFollowing = false;
            if (UserService.IsLoggedIn())
            {
                string userId = UserService.GetCurrentUserId();
                if (userId.Length > 0)
                {
                    DataProvider.ExecuteCmd(GetConnection, "dbo.Followers_SelectByFollowersIdFollowingId"
                       , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                       {
                           paramCollection.AddWithValue("@FollowerUserId", userId);
                           paramCollection.AddWithValue("@FollowingUserId", followedUserId);



                       }, map: delegate (IDataReader reader, short set)
                       {
                           isFollowing = true;


                       }

               );


                }
            }

            return isFollowing;
        }

        public bool DecorateisFollower(string followedUserId)
        {
            bool isFollower = false;
            if (UserService.IsLoggedIn())
            {
                string userId = UserService.GetCurrentUserId();
                if (userId.Length > 0)
                {
                    DataProvider.ExecuteCmd(GetConnection, "dbo.Followers_SelectByFollowersIdFollowingId"
                       , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                       {
                           paramCollection.AddWithValue("@FollowerUserId", followedUserId);
                           paramCollection.AddWithValue("@FollowingUserId", userId);



                       }, map: delegate (IDataReader reader, short set)
                       {
                           isFollower = true;


                       }

               );


                }
            }

            return isFollower;
        }

        public bool DecorateisFollowingAndNotLoggedin(string followedUserId,string userId)
        {
            bool isFollowing = false;
          
                if (userId.Length > 0)
                {
                    DataProvider.ExecuteCmd(GetConnection, "dbo.Followers_SelectByFollowersIdFollowingId"
                       , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                       {
                           paramCollection.AddWithValue("@FollowerUserId", userId);
                           paramCollection.AddWithValue("@FollowingUserId", followedUserId);
                       }, map: delegate (IDataReader reader, short set)
                       {
                           isFollowing = true;

                       }

               );

                }
            

            return isFollowing;
        }

        public PublicUserProfileDomain GetPublicUserByUsername(string username)
        {
            PublicUserProfileDomain PublicUser = null;
            Media Media = null;

            DataProvider.ExecuteCmd(GetConnection, "dbo.AspNetUsers_UserProfile_SelectByUsername"

                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@userName", username);
                }
                , map: delegate (IDataReader reader, short set)
                {
                    PublicUser = new PublicUserProfileDomain();
                    int startingIndex = 0; //startingOrdinal

                    PublicUser._id = reader.GetSafeInt32(startingIndex++);
                    PublicUser.createdDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.followersCount = reader.GetSafeInt32(startingIndex++);
                    PublicUser.followingCount = reader.GetSafeInt32(startingIndex++);
                    PublicUser.firstName = reader.GetSafeString(startingIndex++);
                    PublicUser.lastName = reader.GetSafeString(startingIndex++);
                    PublicUser.profileContent = reader.GetSafeString(startingIndex++);
                    PublicUser.mediaId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.bgmediaId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.pointScore = reader.GetSafeInt32(startingIndex++);
                    PublicUser.locationId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.joinedDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.lastLoggedInDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.tagLine = reader.GetSafeString(startingIndex++);
                    PublicUser.userId = reader.GetSafeString(startingIndex++);
                    PublicUser.Email = reader.GetSafeString(startingIndex++);
                    PublicUser.EmailConfirmed = reader.GetSafeBool(startingIndex++);
                    PublicUser.PhoneNumber = reader.GetSafeString(startingIndex++);
                    PublicUser.UserName = reader.GetSafeString(startingIndex++);

                    Media = new Media();


                    Media.Url = reader.GetSafeString(startingIndex++);
                    Media.Title = reader.GetSafeString(startingIndex++);
                    Media.Description = reader.GetSafeString(startingIndex++);
                    Media.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    Media.DataType = reader.GetSafeString(startingIndex++);
                    Media.Id = reader.GetSafeInt32(startingIndex++);

                    PublicUser.MyMedia = Media;

                    Media = new Media();

                    Media.Url = reader.GetSafeString(startingIndex++);
                    Media.Title = reader.GetSafeString(startingIndex++);
                    Media.Description = reader.GetSafeString(startingIndex++);
                    Media.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    Media.DataType = reader.GetSafeString(startingIndex++);
                    Media.Id = reader.GetSafeInt32(startingIndex++);

                    PublicUser.BgMyMedia = Media;

                    PublicUser.isFollowed = DecorateisFollowing(PublicUser.userId);
                    PublicUser.isFollower = DecorateisFollower(PublicUser.userId);

                }
            );

            return PublicUser;
        }

        //sp updates individual's user's overall point score
        public void UpdateUserPointScore(string userName, int netVote)
        {

            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UserProfile_UpdatePointScore"
               , inputParamMapper: delegate (SqlParameterCollection paramCollection)
               {

                   paramCollection.AddWithValue("@userName", userName);
                   paramCollection.AddWithValue("@NetVote", netVote);
               }

            );
        }


        public void updateProfilePicture(updateUserProfilePicture model)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UpdateUserProfileMedia"
         , inputParamMapper: delegate (SqlParameterCollection paramCollection)
         {

             paramCollection.AddWithValue("@mediaId", model.mediaId);
             paramCollection.AddWithValue("@userId", model.userId);
         }
         );
        }


        public void updateBackgroundPicture(updateUserBackgroundPicture model)
        {
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.UpdateUserBackgroundMedia"
         , inputParamMapper: delegate (SqlParameterCollection paramCollection)
         {

             paramCollection.AddWithValue("@bgmediaId", model.bgmediaId);
             paramCollection.AddWithValue("@userId", model.userId);
         }
         );
        }


        public List<PublicUserProfileDomain> GetUsers(IEnumerable<string> userId)
        {
            List<PublicUserProfileDomain> users = null;
            PublicUserProfileDomain PublicUser = null;
            Media Media = null;
            DataProvider.ExecuteCmd(GetConnection, "dbo.UserProfile_SelectByUsers"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    var p = new SqlParameter("@userIds", SqlDbType.Structured);
                    p.Value = new NVarcharTable(userId);

                    paramCollection.Add(p);
                }, map: delegate (IDataReader reader , short set)
                {
                    PublicUser = new PublicUserProfileDomain();
                    int startingIndex = 0; //startingOrdinal

                    PublicUser._id = reader.GetSafeInt32(startingIndex++);
                    PublicUser.createdDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.followersCount = reader.GetSafeInt32(startingIndex++);
                    PublicUser.followingCount = reader.GetSafeInt32(startingIndex++);
                    PublicUser.firstName = reader.GetSafeString(startingIndex++);
                    PublicUser.lastName = reader.GetSafeString(startingIndex++);
                    PublicUser.profileContent = reader.GetSafeString(startingIndex++);
                    PublicUser.mediaId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.bgmediaId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.pointScore = reader.GetSafeInt32(startingIndex++);
                    PublicUser.locationId = reader.GetSafeInt32(startingIndex++);
                    PublicUser.joinedDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.lastLoggedInDate = reader.GetSafeDateTime(startingIndex++);
                    PublicUser.tagLine = reader.GetSafeString(startingIndex++);
                    PublicUser.userId = reader.GetSafeString(startingIndex++);
                    PublicUser.Email = reader.GetSafeString(startingIndex++);
                    PublicUser.EmailConfirmed = reader.GetSafeBool(startingIndex++);
                    PublicUser.PhoneNumber = reader.GetSafeString(startingIndex++);
                    PublicUser.UserName = reader.GetSafeString(startingIndex++);

                    Media = new Media();


                    Media.Url = reader.GetSafeString(startingIndex++);
                    Media.Title = reader.GetSafeString(startingIndex++);
                    Media.Description = reader.GetSafeString(startingIndex++);
                    Media.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    Media.DataType = reader.GetSafeString(startingIndex++);
                    Media.Id = reader.GetSafeInt32(startingIndex++);

                    PublicUser.MyMedia = Media;

                    Media = new Media();

                    Media.Url = reader.GetSafeString(startingIndex++);
                    Media.Title = reader.GetSafeString(startingIndex++);
                    Media.Description = reader.GetSafeString(startingIndex++);
                    Media.MediaType = reader.GetSafeEnum<MediaType>(startingIndex++);
                    Media.DataType = reader.GetSafeString(startingIndex++);
                    Media.Id = reader.GetSafeInt32(startingIndex++);

                    PublicUser.BgMyMedia = Media;

                    //PublicUser.isFollowed = DecorateisFollowing(PublicUser.userId);
                    //PublicUser.isFollower = DecorateisFollower(PublicUser.userId);

                    if(users == null)
                    {
                        users = new List<PublicUserProfileDomain>();
                    }
                    users.Add(PublicUser);
                }
                );

            var following = FollowersService.CheckIsFollowingUsers(UserService.GetCurrentUserId(), userId);
            var followers = FollowersService.CheckIsUsersFollowing(UserService.GetCurrentUserId(), userId);

            foreach (var user in users)
            {
                user.isFollowed = following.Contains(user.userId);
                user.isFollower = followers.Contains(user.userId);
            }

            return users;
        }

    }
}
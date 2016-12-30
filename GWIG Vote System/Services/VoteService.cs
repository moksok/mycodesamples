using Sabio.Data;
using Sabio.Web.Domain;
using Sabio.Web.Models.Vote;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Sabio.Web.Services
{
    public class VoteService : BaseServiceStatic
    {
        //SP logs individual vote
        public static string VoteInsert(InsertVoteRequest model)
        {

            string userName = "";
            DataProvider.ExecuteNonQuery(GetConnection, "dbo.PointScore_InsertVote"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@UserName", model.UserName);
                    paramCollection.AddWithValue("@NetVote", model.NetVote);
                    paramCollection.AddWithValue("@VoteType", model.VoteType);
                    paramCollection.AddWithValue("@ContentId", model.ContentId);
                    paramCollection.AddWithValue("@VoterId", model.VoterId);
                    paramCollection.AddWithValue("@PlacesId", model.PlacesId);
                    paramCollection.AddWithValue("@UserId", model.UserId);

                }, returnParameters: delegate (SqlParameterCollection param)
                {

                    userName = (string)param["@UserName"].Value;

                });
            return userName;
        }
    }
}
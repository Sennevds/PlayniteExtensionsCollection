﻿using ImporterforAnilist.Models;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Playnite.SDK.Data;
using WebCommon;

namespace ImporterforAnilist.Services
{
    class AnilistAccountClient
    {
        private ILogger logger = LogManager.GetLogger();
        private readonly string apiListQueryString = @"";
        public string anilistUsername = string.Empty;
        public const string GraphQLEndpoint = @"https://graphql.AniList.co";
        private ImporterForAnilistSettingsViewModel settings;
        private const string getUsernameQueryString = @"
query {
    Viewer {
        name
        id
        options {
            displayAdultContent
        }
        mediaListOptions {
            scoreFormat
        }
    }
}";

        private const string updateIdsStatusQuery = @"
mutation ($ids: [Int], $status: MediaListStatus) {
    UpdateMediaListEntries (ids: $ids, status: $status) {
        id
        status
    }
}";
        public bool GetIsLoggedIn()
        {
            anilistUsername = GetUsername();
            if (!anilistUsername.IsNullOrEmpty())
            {
                logger.Info($"AniList username: {anilistUsername}");
                return true;
            }
            else
            {
                return false;
            }
        }

        public AnilistAccountClient(ImporterForAnilistSettingsViewModel settings)
        {
            this.settings = settings;
            this.apiListQueryString = @"
                query GetListByUsername($userName: String!, $type: MediaType!) {
                    list: MediaListCollection(userName: $userName, type: $type, forceSingleCompletedList: true) {
                        lists {
                            status
                            entries {
                                id
                                progress
                                score(format: POINT_100)
                                status
                                updatedAt
                                media {
                                    id
        	                        idMal
        	                        siteUrl	
                                    type 
                                    format
                                    episodes
                                    chapters
                                    averageScore
                                    title {
                                        romaji
                                        english
                                        native
                                    }
                                    description(asHtml: true)
                                    startDate {
                                        year
                                        month
                                        day
                                    }
                                    genres
                                    tags {
                                        name
                                        isGeneralSpoiler
                                        isMediaSpoiler
                                    }
                                    season
                                    status
                                    studios(sort: [NAME]) {
                                        nodes {
                                            name
                                            isAnimationStudio
                                        }
                                    }
                                    staff {
                                        nodes {
                                            name {
                                                full
                                            }
                                        }
                                    }
                                    coverImage {
                                        extraLarge
                                    }
                                    bannerImage
                                }
                            }
                        }
                    }
                }";
        }

        public string GetApiPostResponse(Dictionary<string, string> postParams)
        {
            var headers = new Dictionary<string, string>
            {
                ["Accept"] = "application/json",
                ["Authorization"] = $"Bearer {settings.Settings.AccountAccessCode}"
            };

            var jsonPostContent = Serialization.ToJson(postParams);
            var downloadStringResult = HttpDownloader.DownloadStringFromPostContent(GraphQLEndpoint, jsonPostContent, headers);
            if (downloadStringResult.Success)
            {
                return downloadStringResult.Result;
            }
            else
            {
                logger.Error(downloadStringResult.HttpRequestException, "Failed to process post request.");
                return string.Empty;
            }
        }

        public string GetUsername()
        {
            var postParams = new Dictionary<string, string>
            {
                { "query", getUsernameQueryString },
                { "variables", "" }
            };
            
            var response = GetApiPostResponse(postParams);
            if (string.IsNullOrEmpty(response))
            {
                return string.Empty;
            }
            var anilistUser = Serialization.FromJson<AnilistUser>(response);
            return anilistUser.Data.Viewer?.Name ?? string.Empty;
        }

        public UpdateMediaListEntriesResponse UpdateEntriesStatuses(List<int> Ids, EntryStatus newStatus)
        {
            var vars = @"
            {{
              ""ids"": [{0}],
              ""status"": ""{1}""
            }}";

            var varsEnc = string.Format(vars, string.Join(", ", Ids), newStatus.ToString().ToUpperInvariant());
            var postParams = new Dictionary<string, string>
                {
                    { "query", updateIdsStatusQuery },
                    { "variables", varsEnc }
                };

            var response = GetApiPostResponse(postParams);
            if (response.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                return Serialization.FromJson<UpdateMediaListEntriesResponse>(response);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error during UpdateEntriesStatuses");
                return null;
            }
        }

        public List<Entry> GetEntries(string listType)
        {
            var entriesList = new List<Entry> { }; 
            
            string type = listType.ToUpper();
            var variables = new Dictionary<string, string>
            {
                { "userName", anilistUsername },
                { "type", type }
            };
            string variablesJson = Serialization.ToJson(variables);
            var postParams = new Dictionary<string, string>
            {
                { "query", apiListQueryString },
                { "variables", variablesJson }
            };

            string response = GetApiPostResponse(postParams);
            if (string.IsNullOrEmpty(response))
            {
                return entriesList;
            }

            var mediaList = Serialization.FromJson<MediaList>(response);

            foreach (ListElement listElement in mediaList.Data.List.Lists)
            {
                foreach (Entry entry in listElement.Entries)
                {
                    entriesList.Add(entry);
                }
            }

            return entriesList;
        }
    }
}
﻿using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Flurl.Http;
using Iwentys.Features.Quests.Models;

namespace Iwentys.Endpoint.Sdk.ControllerClients
{
    public class QuestControllerClient
    {
        public QuestControllerClient(HttpClient client)
        {
            Client = client;
        }

        public HttpClient Client { get; }

        public Task<QuestInfoDto> Get(int questId)
        {
            return Client.GetFromJsonAsync<QuestInfoDto>($"/api/quests/{questId}");
        }

        public Task<List<QuestInfoDto>> GetCreatedByUser()
        {
            return Client.GetFromJsonAsync<List<QuestInfoDto>>("/api/quests/created");
        }

        public Task<List<QuestInfoDto>> GetCompletedByUser()
        {
            return Client.GetFromJsonAsync<List<QuestInfoDto>>("/api/quests/completed");
        }

        public Task<List<QuestInfoDto>> GetActive()
        {
            return Client.GetFromJsonAsync<List<QuestInfoDto>>("/api/quests/active");
        }

        public Task<List<QuestInfoDto>> GetArchived()
        {
            return Client.GetFromJsonAsync<List<QuestInfoDto>>("/api/quests/archived");
        }

        public async Task Create(CreateQuestRequest createQuest)
        {
            await Client.PostAsJsonAsync("/api/quests", createQuest);
        }

        public async Task<QuestInfoDto> SendResponse(int questId, QuestResponseCreateArguments arguments)
        {
            IFlurlResponse response = await new FlurlClient(Client)
                .Request($"/api/quests/{questId}/send-response")
                .PostJsonAsync(arguments);

            return await response.GetJsonAsync<QuestInfoDto>();
        }

        public async Task Complete(int questId, QuestCompleteArguments arguments)
        {
            await new FlurlClient(Client)
                .Request($"/api/quests/{questId}/complete")
                .PutJsonAsync(arguments);
        }

        public async Task Revoke(int questId)
        {
            await Client.GetAsync($"/api/quests/{questId}/revoke");
        }

        public async Task<List<QuestRatingRow>> GetQuestExecutorRating()
        {
            return await new FlurlClient(Client)
                .Request($"/api/quests/executor-rating")
                .GetJsonAsync<List<QuestRatingRow>>();
        }
    }
}
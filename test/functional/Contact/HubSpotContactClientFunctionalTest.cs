﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RapidCore.Network;
using HubSpot.Contact;
using HubSpot.Contact.Dto;
using HubSpot.Core.Requests;
using HubSpot.FunctionalTests.Mocks;
using Xunit;
using Xunit.Abstractions;
using HubSpot.FunctionalTests.Mocks.Contact;

namespace HubSpot.FunctionalTests.Contact
{
    public class HubSpotContactClientFunctionalTest : FunctionalTestBase<HubSpotContactClient>
    {
        private readonly HubSpotContactClient _client;

        public HubSpotContactClientFunctionalTest(ITestOutputHelper output)
            : base(output)
        {
            var mockHttpClient = new MockRapidHttpClient()
                .AddTestCase(new CreateContactMockTestCase())
                .AddTestCase(new ListContactMockTestCase())
                .AddTestCase(new GetContactMockTestCase())
                .AddTestCase(new GetContactByEmailMockTestCase())
                .AddTestCase(new GetContactByEmailNotFoundMockTestCase())
                .AddTestCase(new UpdateContactMockTestCase())
                .AddTestCase(new DeleteContactMockTestCase());

            _client = new HubSpotContactClient(
                mockHttpClient,
                Logger,
                new RequestSerializer(new RequestDataConverter(LoggerFactory.CreateLogger<RequestDataConverter>())),
                "https://api.hubapi.com/",
                "HapiKeyFisk"
            );
        }

        [Fact]
        public async Task ContactClient_can_create_contacts()
        {
            var data = await _client.CreateAsync<ContactHubSpotEntity>(new ContactHubSpotEntity
            {
                FirstName = "Mr",
                Lastname = "Test miaki",
                Address = "Æblevej 42",
                City = "Copenhagen",
                ZipCode = "2300",
                Company = "Acme inc",
                Email = "that@email.com",
            });

            Assert.NotNull(data);

            // Should have replied with mocked data, so it does not really correspond to our input data, but it proves the "flow"
            Assert.Equal(61574, data.Id);
        }

        [Fact]
        public async Task ContactClient_can_list_contacts()
        {
            var data = await _client.ListAsync<ContactListHubSpotEntity<ContactHubSpotEntity>>(
                new ContactListRequestOptions
                {
                    NumberOfContactsToReturn = 1
                });

            Assert.NotNull(data);
            Assert.True(data.MoreResultsAvailable);
            Assert.InRange(data.ContinuationOffset, 1, long.MaxValue);
            Assert.True(data.Contacts.Count == 2, "data.Contacts.Count == 2");
        }

        [Fact]
        public async Task ContactClient_can_get_contact()
        {
            const int contactId = 3234574;
            var data = await _client.GetByIdAsync<ContactHubSpotEntity>(contactId);

            Assert.NotNull(data);
            Assert.Equal("Codey", data.FirstName);
            Assert.Equal("Huang", data.Lastname);
            Assert.Equal(contactId, data.Id);
        }

        [Fact]
        public async Task ContactClient_can_get_contact_by_email()
        {
            const string email = "testingapis@hubspot.com";
            var data = await _client.GetByEmailAsync<ContactHubSpotEntity>(email);

            Assert.NotNull(data);
            Assert.Equal("Codey", data.FirstName);
            Assert.Equal("Huang", data.Lastname);
            Assert.Equal(email, data.Email);
        }

        [Fact]
        public async Task ContactClient_returns_null_when_contact_not_found()
        {
            const string email = "not@here.com";
            var data = await _client.GetByEmailAsync<ContactHubSpotEntity>(email);

            Assert.Null(data);
        }

        [Fact]
        public async Task ContactClient_update_contact_works()
        {
            var contact = new ContactHubSpotEntity
            {
                Id = 2340324,
                Email = "new-email@hubspot.com",
                FirstName = "Da First",
                Lastname = "Da last"
            };

            await _client.UpdateAsync(contact);
        }

        [Fact]
        public async Task ContactClient_delete_contact_works()
        {
            await _client.DeleteAsync(61571);
        }
    }
}
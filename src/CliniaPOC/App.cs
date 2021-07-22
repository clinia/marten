using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CliniaPOC.Extensions;
using CliniaPOC.Infrastructure;
using CliniaPOC.Models;
using Marten;
using Marten.Linq.MatchesSql;
using Marten.Pagination;
using Serilog;

namespace CliniaPOC
{
    public class App
    {
        private readonly IDocumentStore _store;
        public static string HealthFacilityId = "0001961a-126b-0cf3-e519-ba0dea0384d3";
        public static string SpecialityId = "6dcd444c-b8fc-3241-9fc6-42dc28a609e7";

        public static string WunshcInc = "wunsch-inc";
        public static string Dialogue = "dialogue";

        public App(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Run()
        {
            #region Fetching

            using var querySession = _store.QuerySession(WunshcInc);

            #region Get single Document

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var singleDocument = await querySession.Query<HealthFacility>()
                .SingleAsync(x => x.Id == HealthFacilityId);

            stopwatch.Stop();

            Log.Information($"Single document id: {singleDocument.Id}, - {stopwatch.Elapsed.ToString()}");

            #endregion

            #region Fetch multiple documents with paging

            stopwatch.Restart();

            var multipleDocuments = await querySession.Query<HealthFacility>()
                .ToPagedListAsync(1, 20);

            Log.Information(
                $"Multiple document with paging count, total count: {multipleDocuments.Count} - {multipleDocuments.TotalItemCount} - {stopwatch.Elapsed}");

            #endregion

            #region Fetch multiple documents with 2 level includes

            stopwatch.Restart();

            var practiceDict = new Dictionary<string, Practice>();
            var practitionerDict = new Dictionary<string, Practitioner>();
            var fetched = await querySession.Query<HealthFacility>()
                .IncludeInverted(x => x.HealthFacility, practiceDict)
                .ThenInclude<Practice, Practitioner, string>(x => x.Practitioner, practitionerDict)
                // We have no other choice than to write where statement targeting dynamic values using raw SQL.
                // This could be abstracted into some compiled queries to facilitate the query process.
                // We should probably need to restrict the amount of possible filters to specific attributes if we don't want to build a fully dynamic query builder for our MVP
                .Where(x => x.MatchesSql("d.data -> 'Values' ->> 'Name' ILIKE ?", "%Braun%"))
                .ToListAsync();

            Log.Information(
                $"Multiple document with 2 level include count, included practitioner count, included practices count, elapsed time: {fetched.Count} - {practitionerDict.Count} - {practiceDict.Count} - {stopwatch.Elapsed.ToString()}");

            #endregion

            #region Fetch multiple documents in batch with 2 level includes

            stopwatch.Restart();

            practiceDict = new Dictionary<string, Practice>();
            practitionerDict = new Dictionary<string, Practitioner>();
            var batchedQuery = querySession.CreateBatchQuery();
            var healthFacilitiesBatchedQueryable = batchedQuery.Query<HealthFacility>()
                .IncludeInverted(x => x.HealthFacility, practiceDict)
                .ThenInclude<Practice, Practitioner, string>(x => x.Practitioner, practitionerDict)
                .SingleOrDefault(x => x.Id == "0001961a-126b-0cf3-e519-ba0dea0384d3");

            var results = new List<Task<HealthFacility>>();
            for (int i = 0; i < 100; i++)
            {
                results.Add(batchedQuery.Query<HealthFacility>()
                    .IncludeInverted(x => x.HealthFacility, practiceDict)
                    .ThenInclude<Practice, Practitioner, string>(x => x.Practitioner, practitionerDict)
                    .SingleOrDefault(x => x.Id == "1"));
            }

            await batchedQuery.Execute();

            var healthFacilitiesBatchedQueryableResult = await healthFacilitiesBatchedQueryable;
            await Task.WhenAll(results);

            Log.Information(
                $"Multiple batch document with 2 level include count, included practitioner count, included practices count, elapsed time: {healthFacilitiesBatchedQueryableResult.Id} - {practitionerDict.Count} - {practiceDict.Count} - {stopwatch.Elapsed.ToString()}");

            #endregion

            #region Fetch multiple documents with 3 level includes with paging

            stopwatch.Restart();

            var healthFacilityDict = new Dictionary<string, HealthFacility>();
            practiceDict = new Dictionary<string, Practice>();
            practitionerDict = new Dictionary<string, Practitioner>();
            var healthServices = await querySession.Query<HealthService>()
                .Include(x => x.HealthFacilityId, healthFacilityDict)
                .ThenIncludeInverted<HealthFacility, Practice, string>(x => x.HealthFacility, practiceDict)
                .ThenInclude<Practice, Practitioner, string>(x => x.Practitioner, practitionerDict)
                .Where(x => x.EqualsString(new [] {"SpecialityId"}, SpecialityId))
                .Take(10)
                .ToListAsync();

            stopwatch.Stop();

            Log.Information(
                $"Multiple document with paging and 3 level include count, included health facilities, included practitioners count, included practices count, elapsed time: {healthServices.Count} - {healthFacilityDict.Count} - {practitionerDict.Count} - {practiceDict.Count} - {stopwatch.Elapsed.ToString()}");

            #endregion

            #region Fetch multiple documents with 3 level includes with compiled query

            stopwatch.Restart();

            healthFacilityDict = new Dictionary<string, HealthFacility>();
            practiceDict = new Dictionary<string, Practice>();
            practitionerDict = new Dictionary<string, Practitioner>();
            healthServices = await querySession.Query<HealthService>()
                .Include(x => x.HealthFacilityId, healthFacilityDict)
                .ThenIncludeInverted<HealthFacility, Practice, string>(x => x.HealthFacility, practiceDict)
                .ThenInclude<Practice, Practitioner, string>(x => x.Practitioner, practitionerDict)
                .Where(x => x.HasSpeciality(SpecialityId))
                .ToListAsync();

            stopwatch.Stop();

            Log.Information(
                $"Multiple document and 3 level include count, included health facilities, included practitioners count, included practices count, elapsed time: {healthServices.Count} - {healthFacilityDict.Count} - {practitionerDict.Count} - {practiceDict.Count} - {stopwatch.Elapsed.ToString()}");

            #endregion

            #region Fetch multiple documents with include mappers

            stopwatch.Restart();

            var healthFacilityQuery = querySession.Query<HealthFacility>();
            var includeMapper = new HealthFacilityIncludeMapper(healthFacilityQuery);
            var healthFacilities = await includeMapper
                .Include(new []{"practices.practitioner", "healthServices"})
                .Take(20)
                .ToListAsync();

            stopwatch.Stop();

            Log.Information(
                $"Multiple document with include mapper, included practices count, included practitioners count, included healthServices count, elapsed time: {healthFacilities.Count} - {includeMapper.Practices.Count} - {includeMapper.Practitioners.Count} - {includeMapper.HealthServices.Count} - {stopwatch.Elapsed.ToString()}");

            #endregion

            #endregion

            #region Creating, updating, deleting

            Log.Information("Creating/Updating/Deleting resources");

            using var createSession = _store.LightweightSession(WunshcInc);

            var healthFacilityId = await CreateHealthFacility(createSession);
            Log.Information($"Created health facility with id: {healthFacilityId}");

            var practitionerId = await CreatePractitioner(createSession);
            Log.Information($"Created practitioner with id: {practitionerId}");

            var practiceId = await CreatePractice(createSession, healthFacilityId, practitionerId);
            Log.Information($"Created practice with id: {practiceId}");

            practiceDict = new Dictionary<string, Practice>();
            practitionerDict = new Dictionary<string, Practitioner>();
            var newlyCreatedHealthFacility = await createSession.Query<HealthFacility>()
                .IncludeInverted(x => x.HealthFacility, practiceDict)
                .ThenInclude<Practice, Practitioner, string>(x => x.Practitioner, practitionerDict)
                .SingleAsync(x => x.Id == healthFacilityId);
            Log.Information($"Fetched health facility with practices + practitioners: {newlyCreatedHealthFacility.Id} - {practiceDict.FirstOrDefault().Value.Id} - {practitionerDict.FirstOrDefault().Value.Id}");

            await UpdateHealthFacility(createSession, healthFacilityId);

            var updatedHealthFacility = await createSession.LoadAsync<HealthFacility>(healthFacilityId);
            Log.Information($"New bathroom count + emails: {updatedHealthFacility.Values["BathroomCount"]} - {(updatedHealthFacility.Values["Emails"] as dynamic[]).First().Value}");

            createSession.Delete<Practice>(practiceId);
            await createSession.SaveChangesAsync();
            var independentHealthFacility = await createSession.LoadAsync<HealthFacility>(healthFacilityId);
            var independentPractitioner = await createSession.LoadAsync<Practitioner>(practitionerId);
            Log.Information($"Deleting practice does not cascade to HealthFacility or Practitioner: {independentPractitioner != null && independentHealthFacility != null}");

            practiceId = await CreatePractice(createSession, healthFacilityId, practitionerId);
            createSession.Delete<Practitioner>(practitionerId);
            await createSession.SaveChangesAsync();
            var practice = await createSession.LoadAsync<Practice>(practiceId);
            Log.Information($"Deleting practitioner cascade delete to practice: {practice == null}");

            #endregion
        }

        public async Task<string> CreateHealthFacility(IDocumentSession session)
        {
            var values = new Dictionary<string, object>
            {
                {"Name", "Clinique du pied"},
                {
                    "Address", new
                    {
                        StreetNumber = 2241,
                        Route = "Asselin Street",
                        Place = "Longueuil",
                        Region = "QC",
                        Country = "Canada"
                    }
                },
                {
                    "Phones",
                    new[]
                    {
                        new {Value = "5146072767", Use = "main", System = "phone"},
                        new {Value = "5555555555", Use = "main", System = "fax"}
                    }
                },
                {"BathroomCount", 2}
            };
            var healthFacility = new HealthFacility("pharmacy") {Values = values};
            session.Store(healthFacility);
            await session.SaveChangesAsync();

            return healthFacility.Id;
        }

        public async Task UpdateHealthFacility(IDocumentSession session, string healthFacilityId)
        {
            var healthFacility = await session.LoadAsync<HealthFacility>(healthFacilityId);

            healthFacility.Values["BathroomCount"] = 3;
            healthFacility.Values["Emails"] = new[] {new {Value = "test@clinia.com"}};

            session.Store(healthFacility);
            await session.SaveChangesAsync();
        }

        public async Task<string> CreatePractitioner(IDocumentSession session)
        {
            var values = new Dictionary<string, object>
            {
                {"FirstName", "John"},
                {"LastName", "Favreau"},
                {
                    "Phones",
                    new[]
                    {
                        new {Value = "5146072767", Use = "main", System = "phone"},
                        new {Value = "5555555555", Use = "main", System = "fax"}
                    }
                },
                {"Age", 50}
            };

            var practitioner = new Practitioner("practitioner") {Values = values};
            session.Store(practitioner);
            await session.SaveChangesAsync();

            return practitioner.Id;
        }

        public async Task<string> CreatePractice(IDocumentSession session,
            string healthFacilityId,
            string practitionerId)
        {
            var values = new Dictionary<string, object>
            {
                {"Services", new[] {"service1", "service2"}},
            };
            var practice = new Practice("practice")
            {
                Values = values,
                HealthFacility = healthFacilityId,
                Practitioner = practitionerId
            };
            session.Store(practice);
            await session.SaveChangesAsync();

            return practice.Id;
        }
    }
}

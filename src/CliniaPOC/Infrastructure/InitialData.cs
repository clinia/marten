using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using CliniaPOC.Models;
using Marten;
using Marten.Schema;

namespace CliniaPOC.Infrastructure
{
    public class InitialData : IInitialData
    {
        private const int Size = 100;

        public void Populate(IDocumentStore store)
        {
            store.Advanced.Clean.CompletelyRemoveAll();
            store.Schema.ApplyAllConfiguredChangesToDatabase(AutoCreate.All);

            var idFaker = new Faker();
            var healthFacilityIds = idFaker.Make(Size - 1, () => idFaker.Random.Guid().ToString()).Concat(new [] {App.HealthFacilityId}).ToArray();
            var practitionerIds = idFaker.Make(Size, () => idFaker.Random.Guid().ToString()).ToArray();
            var specialityIds = idFaker.Make(99, () => idFaker.Random.Guid().ToString()).Concat(new []{App.SpecialityId}).ToArray();

            var index = 0;
            var practiceFaker = new Faker<Practice>()
                .CustomInstantiator(f => new Practice("practice"))
                .RuleFor(x => x.Practitioner, f => practitionerIds[f.Random.Int(0, Size - 1)])
                .RuleFor(x => x.HealthFacility, f => healthFacilityIds[(int)Math.Floor((double)index++ / 10)]);
            var practices = practiceFaker.Generate(Size * 10);

            index = 0;
            var healthFacilityFaker = new Faker<HealthFacility>()
                .CustomInstantiator(f => new HealthFacility("clinic"))
                .RuleFor(x => x.Id, f => healthFacilityIds[index++])
                .RuleFor(x => x.Values, f => new Dictionary<string, object>
                {
                    {"Name", f.Person.Company.Name}
                });
            var healthFacilities = healthFacilityFaker.Generate(Size);

            index = 0;
            var practitionerFaker = new Faker<Practitioner>()
                .CustomInstantiator(f => new Practitioner("practitioner"))
                .RuleFor(x => x.Id, f => practitionerIds[index++])
                .RuleFor(x => x.Values, f => new Dictionary<string, object>
                {
                    {"FirstName", f.Person.FirstName},
                    {"LastName", f.Person.LastName}
                });
            var practitioners = practitionerFaker.Generate(Size);

            index = 0;
            var healthServiceFaker = new Faker<HealthService>()
                .CustomInstantiator(f => new HealthService("speciality"))
                .RuleFor(x => x.Values, f => new Dictionary<string, object>
                {
                    {"SpecialityId", specialityIds[f.Random.Int(0, 99)]}
                })
                .RuleFor(x => x.HealthFacilityId, f => healthFacilityIds[(int)Math.Floor((double)index++ / 10)]);
            var healthServices = healthServiceFaker.Generate(Size * 10);

            using var session = store.LightweightSession(App.WunshcInc);
            session.Store<Practitioner>(practitioners);
            session.Store<HealthFacility>(healthFacilities);
            session.Store<Practice>(practices);
            session.Store<HealthService>(healthServices);
            session.SaveChanges();

            using var session2 = store.LightweightSession(App.Dialogue);
            session2.Store<Practitioner>(practitioners);
            session2.Store<HealthFacility>(healthFacilities);
            session2.Store<Practice>(practices);
            session2.Store<HealthService>(healthServices);
            session2.SaveChanges();
        }
    }
}

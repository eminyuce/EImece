using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutoMapper;
using System.Collections.Generic;

namespace EImece.Tests.Controllers
{
    [TestClass]
    public class AutoMapUnitTest
    {
        [TestInitialize]
        public void Setup()
        {
            Mapper.Initialize(cfg => {
                cfg.AddProfile<MapProfile>();
                cfg.CreateMap<Team, TeamDTO>()
                .ForMember(x => x.Name3, x => x.Ignore()).ReverseMap();
                cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
                cfg.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
            });
        }

        [TestMethod]
        public void TestMethod1()
        {
          
            var team = new Team();
            team.Id = 1;
            team.Name = "EMIN YUCE";
            team.OrganizationDate = DateTime.Now.AddMonths(-2);
            var teamDto = Mapper.Map<TeamDTO>(team);
           
            Console.WriteLine(teamDto);
        }
    }
    public class MapProfile : Profile
    {
      
    }
    //Mapping

  

    //Logical
    public class TestClassPLogical
    {
        public List<TestClassLogical> TestProp
        {
            get;
            set;
        }
    }

    public class TestClassLogical
    {
        //defined prop
    }

    //Model

    public class TestClassPDto
    {
        public virtual IEnumerable<TestClassDto> TestProp
        {
            get;
            set;
        }
    }

    public class TestClassDto
    {
        //defined prop
    }
    public class Team
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime OrganizationDate { get; set; }

        public string Name2 { get; set; }
    }
    public class TeamDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime OrganizationDate { get; set; }
        public string Name3 { get; set; }

        public override string ToString()
        {
            return Id + " " + Name + " " + OrganizationDate.ToLongDateString();
        }
    }
}

using AutoMapper;
using EImece.Domain;
using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace EImece.Tests.Controllers
{
    [TestClass]
    [DeploymentItem("EntityFramework.SqlServer.dll")]
    public class AutoMapUnitTest
    {
        [TestInitialize]
        public void Setup()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<MapProfile>();
                cfg.CreateMap<Product, ProductDTO>();
                cfg.CreateMap<Team, TeamDTO>()
                .ForMember(x => x.Name3, x => x.Ignore()).ReverseMap();
                //cfg.CreateMap<ApplicantSkillVM, ApplicantSkill>().ForMember(x => x.Skill, x => x.Ignore()).ReverseMap();
                cfg.CreateMap<ApplicantSkillVM, ApplicantSkill>().ReverseMap();
                cfg.CreateMap<ApplicantVM, Applicant>().ReverseMap();
                cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
                cfg.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
            });
        }

        private String ConnectionString { get { return Constants.DbConnectionKey; } }

        [TestMethod]
        public void TestMethodProduct()
        {
            var context = new EImeceContext(ConnectionString);

            var ProductList = context.Products
             .Where(p => p.Name.StartsWith("ven"))
             .ToList();

            var ProductDTOList = ProductList.Select(p => Mapper.Map<ProductDTO>(p));
            foreach (var item in ProductDTOList)
            {
                Console.WriteLine(item.Name);
            }
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

        #region Methods

        [TestMethod]
        public void TestMethod2()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMissingTypeMaps = true);
            var mapper = config.CreateMapper();

            ApplicantVM ap = new ApplicantVM
            {
                Name = "its me",
                ApplicantSkills = new List<ApplicantSkillVM>
                {
                    new ApplicantSkillVM {SomeInt = 11, SomeString = "test1", Skill = new Skill {SomeInt = 20}},
                    new ApplicantSkillVM {SomeInt = 12, SomeString = "test2"}
                }
            };

            List<ApplicantVM> applicantVms = new List<ApplicantVM> { ap };
            // Map
            List<Applicant> apcants = Mapper.Map<List<ApplicantVM>, List<Applicant>>(applicantVms);
            Console.WriteLine(apcants.Count);
        }

        #endregion Methods
    }

    /// Your source classes
    public class Applicant
    {
        #region Properties

        public List<ApplicantSkill> ApplicantSkills { get; set; }

        public string Name { get; set; }

        #endregion Properties
    }

    public class ApplicantSkill
    {
        #region Properties

        public Skill Skill { get; set; }

        public int SomeInt { get; set; }
        public string SomeString { get; set; }

        #endregion Properties
    }

    // Your VM classes
    public class ApplicantVM
    {
        #region Properties

        public List<ApplicantSkillVM> ApplicantSkills { get; set; }

        public string Description { get; set; }
        public string Name { get; set; }

        #endregion Properties
    }

    public class ApplicantSkillVM
    {
        #region Properties

        public Skill Skill { get; set; }

        public int SomeInt { get; set; }
        public string SomeString { get; set; }

        #endregion Properties
    }

    public class Skill
    {
        #region Properties

        public int SomeInt { get; set; }

        #endregion Properties
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
        public int SomeInt { get; set; }
        public string SomeString { get; set; }
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime OrganizationDate { get; set; }

        public string Name2 { get; set; }

        public override string ToString()
        {
            return Id + " " + Name + " " + OrganizationDate.ToLongDateString() + " " + SomeInt + " " + SomeString;
        }
    }

    public class ProductDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; }
        //public int Position { get; set; }
        //public int Lang { get; set; }
        //public string Description { get; set; }
        //public Boolean ImageState { get; set; }
        //public string MetaKeywords { get; set; }
        //public int? MainImageId { get; set; }
        //public int ImageHeight { get; set; }
        //public int ImageWidth { get; set; }
        //public string UpdateUserId { get; set; }
        //public string AddUserId { get; set; }
        //public string NameShort { get; set; }
        //public int ProductCategoryId { get; set; }
        //public Boolean MainPage { get; set; }
        //public double Price { get; set; }
        //public double Discount { get; set; }
        //public string ProductCode { get; set; }
        //public string VideoUrl { get; set; }
        //public Boolean IsCampaign { get; set; }
    }

    [DataContract]
    public class TeamDTO
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime OrganizationDate { get; set; }

        [DataMember]
        public string Name3 { get; set; }

        public override string ToString()
        {
            return Id + " " + Name + " " + OrganizationDate.ToLongDateString();
        }
    }
}
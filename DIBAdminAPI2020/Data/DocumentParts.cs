using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DIBAdminAPI.Services;
using DIBAdminAPI.Data.Entities;

namespace DIBAdminAPI.Data
{
    public class DocumentParts : IDocumentParts
    {
        private readonly IRepository _repo;
        private readonly ICacheService _cache;
        public DocumentParts(IRepository repository, ICacheService cacheService)
        {
            _repo = repository;
            _cache = cacheService;
        }
        private class DocumentParameters
        {
            public string resourceId { get; set; }
            public string segmentId { get; set; }
            public int tag3 { get; set; }
            public DocumentParameters() { }
        }
        public async Task<List<DocumentContainer>> GetParts(string resourceId, string segmentId, string id)
        {
            List<DocumentContainer> result = new List<DocumentContainer>();
            string session_id = "apitest";//_usrsvc.CurrentUser.session_id

            if ((segmentId ?? "") == "diblink")
            {
                if ((id ?? "") == "") return null;
                var p = new
                {
                    id,
                    session_id    
                };
                    
                IEnumerable<DIBLink> links = await _repo.GetResourceDibLinkData(p);

                
                List<DocumentParameters> segments =
                (
                    links
                    .Select(p => new {p.resourceId,  segmentId = p.segmentId, idx = (p.idx??0), tag3 = (p.tag3??"")})
                    .Union(
                        links
                        .Where(p => p.tosegmentId != null)
                        .Select(p => new {p.resourceId, segmentId = p.tosegmentId, idx = (p.toidx??0), tag3=(p.totag3??"")})
                    )
                ).ToList()
                .OrderBy(p=>p.idx)
                .GroupBy(p => new DocumentParameters{ resourceId= p.resourceId, segmentId= p.segmentId, tag3 = p.tag3.Length } )
                .Select(p => p.Key)
                .ToList();

                foreach (DocumentParameters s in segments)
                {
                    
                    if (s.tag3 >0)
                    {

                    }
                    var p1 = new
                    {
                        session_id,
                        id = s.resourceId,
                        segment_id = s.segmentId,
                        collection_id = ""

                    };
                    ResourceHTML5 data = await _repo.GetHTML5("[dbo].[GetResourceHTML]", p, null);

                    List<DIBLink> resourceLinks = links
                        .Where(p => p.resourceId == s.resourceId && p.segmentId == s.segmentId)
                        .OrderBy(p => p.idx)
                        .ToList();
                        


                }

                return null;
            }
            else
            {
                return null;
            }
            
        }
    }
}

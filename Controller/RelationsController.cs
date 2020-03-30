using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace BizBraco.Controllers
{

  [UmbracoApplicationAuthorize("content")]
  [PluginController("RelationsManager")]
  public class RelationsController : UmbracoAuthorizedApiController
  {

    private readonly IRelationService relationService;

    public RelationsController()
    {
      relationService = Current.Services.RelationService;
    }


    /// <summary>
    /// Returns a list of object that we can relate with current object
    /// </summary>
    /// <param name="OggettiRelazione"></param>
    /// <param name="RelationTypeAlias"></param>
    /// <param name="CurrentObjId"></param>
    /// <returns></returns>
    [System.Web.Http.HttpPost]
    public List<RelatedObjectVm> GetObjectsToRelate(string OggettiRelazione, string RelationTypeAlias,int CurrentObjId)
    {

      //Lista DocumentypeAlias coinvolti nella relazione
      var objRel = JsonConvert.DeserializeObject<List<string>>(OggettiRelazione);

      //Lista di oggetti con DocumentypeAlias contenuto nella lista precedente
      var objects = Umbraco.ContentAtRoot().First().DescendantsOrSelf().Where(x => objRel.Contains(x.ContentType.Alias)).Select(x=>new RelatedObjectVm{Id=x.Id,Name=x.Name}).ToList();

      //Oggetti già relazionati con l'id dell'oggetto in input
      var alreadyRelated = GetRelatedObjects(RelationTypeAlias, CurrentObjId);

      //Oggetti rimanenti
      objects = objects.Where(x => !alreadyRelated.Select(k => k.Id).Contains(x.Id)).ToList();

      return objects;

    }

    /// <summary>
    /// Ritorna la lista di oggetti relazionati con l'oggetto richiesto
    /// </summary>
    /// <param name="RelationTypeAlias"></param>
    /// <param name="CurrentObjId"></param>
    /// <returns></returns>
    [System.Web.Http.HttpGet]
    public List<RelatedObjectVm> GetRelatedObjects(string RelationTypeAlias, int CurrentObjId)
    {

      var relType = relationService.GetRelationTypeByAlias(RelationTypeAlias);

      //Ottengo tutte le relazioni
      var relationsParents = relationService.GetByParentId(CurrentObjId).Where(x => x.RelationType.Id == relType.Id);
      var relationsChilds = relationService.GetByChildId(CurrentObjId).Where(x => x.RelationType.Id == relType.Id);

      var relatedObjects = relationsParents.Select(x => new KeyValuePair<int,int>(x.ChildId,x.Id)).ToList();
      relatedObjects.AddRange(relationsChilds.Select(x => new KeyValuePair<int, int>(x.ParentId, x.Id)));


      //Genero la lista formattata per il frontend
      var final = new List<RelatedObjectVm>();

      foreach (var rel in relatedObjects)
      {

        var umbObj = Umbraco.Content(rel.Key);

        if(umbObj != null)
        {
          final.Add(new RelatedObjectVm { Id = umbObj.Id, Name = umbObj.Name, RelationId = rel.Value });
        }
        
      }

      return final;

    }


    /// <summary>
    /// Aggiunge una relazione tra l'oggetto richiesto e l'oggetto da relazionare
    /// </summary>
    /// <param name="relTypeAlias"></param>
    /// <param name="curObjId"></param>
    /// <param name="relObjId"></param>
    /// <returns></returns>
    [System.Web.Http.HttpGet]
    [System.Web.Http.AcceptVerbs("GET")]
    public RelatedObjectVm AddRelation(string relTypeAlias, int curObjId, int relObjId)
    {
      var newRel = relationService.Relate(curObjId, relObjId, relTypeAlias);

      var umbObj = Umbraco.Content(relObjId);

      return new RelatedObjectVm{Id = umbObj.Id, Name = umbObj.Name, RelationId = newRel.Id};

    }

    /// <summary>
    /// Elimina una relazione con id specifico
    /// </summary>
    /// <param name="relationId"></param>
    [System.Web.Http.HttpGet]
    [System.Web.Http.AcceptVerbs("GET")]
    public void DeleteRelation(int relationId)
    {

      var relationToDelete = relationService.GetById(relationId);
      
      relationService.Delete(relationToDelete);

    }

    
    public class RelatedObjectVm
    {
      public int Id { get; set; }
      public string Name { get; set; }
      public int RelationId { get; set; }
    }

  }
}
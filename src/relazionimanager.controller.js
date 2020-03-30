//Resources function
function RelationResources($http, umbRequestHelper) {

    //Define root path for ajax call
    var root = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath + "/backoffice/RelationsManager/relations/";

    return {

        /**
         * Get all objects we can relate to current object.
         * 
         * @param {string} objalias 
         * @param {string} relTypeAlias 
         * @param {int} curObjId 
         */
        GetObjectsToRelate: function (objalias, relTypeAlias, curObjId) {

            return umbRequestHelper.resourcePromise(
                $http({
                    url: root + "GetObjectsToRelate?RelationTypeAlias=" + relTypeAlias + "&CurrentObjId=" + curObjId + "&OggettiRelazione="+JSON.stringify(objalias),
                    method: "POST"
                }), "Errore GetObjectsToRelate");
        },

        /**
         * Get all objects already related to current object.
         * 
         * @param {string} relTypeAlias 
         * @param {int} curObjId 
         */
        GetRelatedObjects: function (relTypeAlias, curObjId) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    root + "GetRelatedObjects?RelationTypeAlias=" + relTypeAlias + "&CurrentObjId=" + curObjId),
                "Errore GetRelatedObjects"
            );
        },

        /**
         * Add a new relation between current object and selected object
         * 
         * @param {string} relTypeAlias 
         * @param {int} curObjId 
         * @param {int} relObjId 
         */
        AddRelation: function (relTypeAlias, curObjId, relObjId) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    root + "AddRelation?relTypeAlias=" + relTypeAlias + "&curObjId=" + curObjId +"&relObjId="+relObjId),
                "Errore GetRelatedObjects"
            );
        },

        /**
         *  Delete relation between current object and selected object
         * 
         * @param {int} RelationId 
         */
        DeleteRelation: function (RelationId) {
            return umbRequestHelper.resourcePromise(
                $http.get(
                    root + "DeleteRelation?relationId=" + RelationId),
                "Errore GetRelatedObjects"
            );
        }
    };

}

function EditRelationsController($scope, $routeParams,relationResources, assetsService) {

    //Basic configuration

    if ($scope.model.value === null || $scope.model.value === "") {
        $scope.model.value = $scope.model.config.defaultValue;
    }

	 $scope.filterSearch = "";
	 $scope.filterOTRSearch = "";
	
    var AliasOggettiRelazione = [];

    if ($scope.model.config.OggettoRelazione.includes(',')) {
        AliasOggettiRelazione = $scope.model.config.OggettoRelazione.split(',');
    } else {
        AliasOggettiRelazione.push($scope.model.config.OggettoRelazione);
    }

    var RelationTypeAlias = $scope.model.config.RelationAlias;
    var CurrentObjId = $routeParams.id;
    
    //Plugin functions

	$scope.filterObjects = function (search) {
		
		if(search != ""){
			$scope.RelatedObjects.forEach(obj => obj.hide = !obj.Name.toLowerCase().includes(search.toLowerCase()));
		}else{
			$scope.RelatedObjects.forEach(obj => obj.hide = false);
		}

    };
	
	$scope.filterObjectsToRelate = function (search) {
		
		if(search != ""){
			$scope.ObjectsToRelate.forEach(obj => obj.hide = !obj.Name.toLowerCase().includes(search.toLowerCase()));
		}else{
			$scope.ObjectsToRelate.forEach(obj => obj.hide = false);
		}

    };
	
    $scope.Add = function (relObjId) {
        relationResources.AddRelation(RelationTypeAlias, CurrentObjId, relObjId).then(function(data) {

            $scope.RelatedObjects.push(data);

            $scope.ObjectsToRelate = $scope.ObjectsToRelate.filter(obj => obj.Id !== data.Id);

        });
    };

    $scope.Delete = function (relationId, delObjId, delObjName) {
        relationResources.DeleteRelation(relationId).then(function (data) {

            $scope.ObjectsToRelate.push({ Id: delObjId, Name: delObjName});

            $scope.RelatedObjects = $scope.RelatedObjects.filter(obj => obj.Id !== delObjId);

        });
    };

    //Internal functions

    function InitPlugin() {
       

        relationResources.GetObjectsToRelate(AliasOggettiRelazione, RelationTypeAlias, CurrentObjId).then(function (data) {
            $scope.ObjectsToRelate = data;
        });

        relationResources.GetRelatedObjects(RelationTypeAlias, CurrentObjId).then(function (data) {

            $scope.RelatedObjects = data;

        });
    }

    //Run plugin initializazion
    InitPlugin();
    
}

//Setup
angular.module("umbraco")
    .factory("RelazioniManager.RelationResources", ["$http", "umbRequestHelper", RelationResources])
    .controller("RelazioniManager.EditRelationsController", [
    "$scope",
    "$routeParams",
    "RelazioniManager.RelationResources",
    "assetsService",
    EditRelationsController]);
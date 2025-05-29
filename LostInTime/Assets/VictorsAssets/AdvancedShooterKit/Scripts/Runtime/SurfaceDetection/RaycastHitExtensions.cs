/********************************************
 * Copyright(c): 2018 Victor Klepikov       *
 *                                          *
 * Profile: 	 http://u3d.as/5Fb		    *
 * Support:      http://smart-assets.org    *
 ********************************************/


using UnityEngine;

namespace AdvancedShooterKit
{
    // Extensions for RaycastHit, Material, Texture
    public static class RaycastHitExtensions
    {
        // Get SurfaceName ByHit
        public static string GetSurface( this RaycastHit hitInfo )
        {
            return SurfaceDetector.GetSurface( hitInfo );
        }

        // Get SurfaceName ByMaterial
        public static string GetSurface( this Material meshMaterial )
        {
            return SurfaceDetector.GetSurface( meshMaterial );
        }

        // Get SurfaceName ByTexture
        public static string GetSurface( this Texture terrainTexture )
        {
            return SurfaceDetector.GetSurface( terrainTexture );
        }


        // Try GetSurface ByHit
        public static bool TryGetSurface( this RaycastHit hit, out string surface )
        {
            return SurfaceDetector.TryGetSurface( hit, out surface );
        }

        // Try GetSurface ByHit
        public static bool TryGetSurface( this Material meshMaterial, out string surface )
        {
            return SurfaceDetector.TryGetSurface( meshMaterial, out surface );
        }

        // Try GetSurface ByHit
        public static bool TryGetSurface( this Texture terrainTexture, out string surface )
        {
            return SurfaceDetector.TryGetSurface( terrainTexture, out surface );
        }




        // Get Material ByHit
        public static Material GetMaterial( this RaycastHit hitInfo )
        {
            if( hitInfo.collider == null || hitInfo.collider.isTrigger || hitInfo.collider is TerrainCollider )
            {
                return null;
            }

            Renderer renderer = hitInfo.collider.GetComponent<Renderer>();
            if( renderer == null )
            {
                return null;
            }

            MeshCollider meshCollider = hitInfo.collider as MeshCollider;
            if( meshCollider == null || meshCollider.convex )
            {
                return renderer.sharedMaterial;
            }

            Mesh sharedMesh = meshCollider.sharedMesh;
            int hitIndex = hitInfo.triangleIndex * 3;

            for( int meshId = 0; meshId < sharedMesh.subMeshCount; meshId++ )
            {
                int trianglesLength = sharedMesh.GetTriangles( meshId ).Length;

                if( hitIndex < trianglesLength )
                {
                    return renderer.sharedMaterials[ meshId ];
                }
                else
                {
                    hitIndex -= trianglesLength;
                }
            }

            return null;
        }

        // Get TerrainTexture ByHit
        public static Texture GetTerrainTexture( this RaycastHit hitInfo )
        {
            if( hitInfo.collider == null || hitInfo.collider.isTrigger )
            {
                return null;
            }


            TerrainCollider col = hitInfo.collider as TerrainCollider;

            if( col == null || col.terrainData == null )
            {
                return null;
            }

            TerrainData terData = col.terrainData;
            Vector3 terrainPos = hitInfo.transform.position;
            int mapX = Mathf.RoundToInt( ( ( hitInfo.point.x - terrainPos.x ) / terData.size.x ) * terData.alphamapWidth );
            int mapZ = Mathf.RoundToInt( ( ( hitInfo.point.z - terrainPos.z ) / terData.size.z ) * terData.alphamapHeight );
            float[,,] splatmapData = terData.GetAlphamaps( mapX, mapZ, 1, 1 );
            SplatPrototype[] splatPrototypes = terData.splatPrototypes;

            for( int i = 0; i < splatPrototypes.Length; i++ )
            {
                if( splatmapData[ 0, 0, i ] > .5f )
                    return splatPrototypes[ i ].texture;
            }

            return null;
        }


        // TryGet Material ByHit
        public static bool TryGetMaterial( this RaycastHit hitInfo, out Material meshMaterial )
        {
            meshMaterial = hitInfo.GetMaterial();
            return ( meshMaterial != null );
        }

        // TryGet TerrainTexture ByHit
        public static bool TryGetTerrainTexture( this RaycastHit hitInfo, out Texture terrainTexture )
        {
            terrainTexture = hitInfo.GetTerrainTexture();
            return ( terrainTexture != null );
        }
    };
}

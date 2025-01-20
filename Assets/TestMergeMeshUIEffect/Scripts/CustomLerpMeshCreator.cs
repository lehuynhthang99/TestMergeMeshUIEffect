using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ShowOdinSerializedPropertiesInInspector]
public class CustomLerpMeshCreator : MaskableGraphic, ISerializationCallbackReceiver
{
    [SerializeField]
    Texture m_Texture;

    [SerializeField]
    Vector2 _offset = new Vector2(15, 15);

    [SerializeField]
    int[] _clrIndexes;

    [SerializeField] private bool _isEnable = true;
    [SerializeField] private Color _colorDisable;


    [NonSerialized]
    Vector4[,] _uvMapEdge = new Vector4[,]
    {
        //BotSection
        { new Vector4(0, 0,        18.0f/116, 18.0f/72), new Vector4(18.0f/116, 0,        25.0f/116, 18.0f/72), new Vector4(25.0f/116, 0,        43.0f/116, 18.0f/72), },
         //MidSection
        { new Vector4(0, 18.0f/72, 18.0f/116, 22.0f/72), new Vector4(18.0f/116, 18.0f/72, 25.0f/116, 22.0f/72), new Vector4(25.0f/116, 18.0f/72, 43.0f/116, 22.0f/72), },
        //TopSection
        { new Vector4(0, 22.0f/72, 18.0f/116, 40.0f/72), new Vector4(18.0f/116, 22.0f/72, 25.0f/116, 40.0f/72), new Vector4(25.0f/116, 22.0f/72, 43.0f/116, 40.0f/72), },
    };

    [NonSerialized]
    Vector4[,] _uvMapInsideCorner = new Vector4[,]
    {
        //Inside left bot corner section
        {
            new Vector4(44.0f/116, 0,        62.0f/116, 18.0f/72), new Vector4(62.0f/116, 0,        80.0f/116, 18.0f/72),
            new Vector4(80.0f/116, 0,        98.0f/116, 18.0f/72), new Vector4(98.0f/116, 0,        116.0f/116, 18.0f/72),
        },

        //Inside right bot corner section
        {
            new Vector4(44.0f/116, 18.0f/72, 62.0f/116, 36.0f/72), new Vector4(62.0f/116, 18.0f/72, 80.0f/116, 36.0f/72),
            new Vector4(80.0f/116, 18.0f/72, 98.0f/116, 36.0f/72), new Vector4(98.0f/116, 18.0f/72, 116.0f/116, 36.0f/72),
        },
        
        //Inside left top corner section
        {
            new Vector4(44.0f/116, 36.0f/72, 62.0f/116, 54.0f/72), new Vector4(62.0f/116, 36.0f/72, 80.0f/116,  54.0f/72),
            new Vector4(80.0f/116, 36.0f/72, 98.0f/116, 54.0f/72), new Vector4(98.0f/116, 36.0f/72, 116.0f/116,  54.0f/72),
        },
        
        //Inside left top corner section
        {
            new Vector4(44.0f/116, 54.0f/72, 62.0f/116, 72.0f/72), new Vector4(62.0f/116, 54.0f/72, 80.0f/116,  72.0f/72),
            new Vector4(80.0f/116, 54.0f/72, 98.0f/116, 72.0f/72), new Vector4(98.0f/116, 54.0f/72, 116.0f/116,  72.0f/72),
        },
    };

    [NonSerialized]
    float[] _xPos = new float[4];

    [NonSerialized]
    float[] _yPos = new float[4];

    [TableMatrix(SquareCells = true), SerializeField]
    bool[,] _tileNextToItem = new bool[3, 3];

    [NonSerialized]
    Vector4[,] _regionPos = new Vector4[3, 3];

    [NonSerialized]
    bool _isClearPosMesh = true;

    [NonSerialized]
    int _paramPack;


    // make it such that unity will trigger our ui element to redraw whenever we change the texture in the inspector
    public Texture texture
    {
        get
        {
            return m_Texture;
        }
        set
        {
            if (m_Texture == value)
                return;

            m_Texture = value;
            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
        SetMaterialDirty();
    }

    // if no texture is configured, use the default white texture as mainTexture
    public override Texture mainTexture
    {
        get
        {
            return m_Texture == null ? s_WhiteTexture : m_Texture;
        }
    }

    // helper to easily create quads for our ui mesh. You could make any triangle-based geometry other than quads, too!
    void AddQuad(VertexHelper vh, Vector2 corner1, Vector2 corner2, Vector2 uvCorner1, Vector2 uvCorner2)
    {
        float rectWidth = rectTransform.rect.width;
        float rectHeight = rectTransform.rect.height;
        Vector2 size = new Vector2(rectWidth, rectHeight);
        Vector2 delta = Vector2.one * 0.5f;
        var i = vh.currentVertCount;

        UIVertex vert = new UIVertex();

        Color colorTint = _isEnable ? this.color : this.color * (1 - _colorDisable.a) + _colorDisable * _colorDisable.a;
        colorTint.a = this.color.a;
        vert.color = colorTint;
        vert.uv1 = new Vector4(_paramPack, 0, 0, 0);

        vert.position = corner1;
        vert.uv0 = new Vector2(uvCorner1.CompressUVVector(), (vert.position / size + delta).CompressUVVector());
        //vert.uv1 = (vert.position / size + delta);
        vh.AddFull(vert);

        vert.position = new Vector2(corner2.x, corner1.y);
        vert.uv0 = new Vector2(new Vector2(uvCorner2.x, uvCorner1.y).CompressUVVector(), (vert.position / size + delta).CompressUVVector());
        //vert.uv1 = (vert.position / size + delta);
        vh.AddFull(vert);

        vert.position = corner2;
        vert.uv0 = new Vector2(uvCorner2.CompressUVVector(), (vert.position / size + delta).CompressUVVector());
        //vert.uv1 = (vert.position / size + delta);
        vh.AddFull(vert);

        vert.position = new Vector2(corner1.x, corner2.y);
        vert.uv0 = new Vector2(new Vector2(uvCorner1.x, uvCorner2.y).CompressUVVector(), (vert.position / size + delta).CompressUVVector());
        //vert.uv1 = (vert.position / size + delta);
        vh.AddFull(vert);

        vh.AddTriangle(i + 0, i + 2, i + 1);
        vh.AddTriangle(i + 3, i + 2, i + 0);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        var bottomLeftCorner = new Vector2(0, 0) - rectTransform.pivot;
        bottomLeftCorner.x *= rectTransform.rect.width;
        bottomLeftCorner.y *= rectTransform.rect.height;

        _paramPack = CreateParamShader();
        CheckAndCreateNewRegionPos();

        //x,y: bottom left corner; z,w: top right corner
        //Vector4 border = new Vector4(bottomLeftCorner.x, bottomLeftCorner.y, bottomLeftCorner.x + rect.width, bottomLeftCorner.y + rect.height);

        for (int yIndex = -1; yIndex < 2; yIndex++)
            for (int xIndex = -1; xIndex < 2; xIndex++)
            {
                int postionByEdge = Mathf.Abs(xIndex) + Mathf.Abs(yIndex);

                //right at the center
                if (postionByEdge == 0)
                {
                    AddCenterQuad(vh);
                }
                //this is the edge
                else if (postionByEdge == 1)
                {
                    AddEdgeQuad(vh, xIndex, yIndex);
                }
                //otherwise == 2 --> this is the corner
                else
                {
                    AddCornerQuad(vh, xIndex, yIndex);
                }

                //  Vector4 curRegionPos = _regionPos[xIndex, yIndex];
                //  Vector4 uvMap = _uvMapSection[xIndex + yIndex * 3];

                //  AddQuad(vh,
                //new Vector2(curRegionPos.x, curRegionPos.y),
                //new Vector2(curRegionPos.z, curRegionPos.w),
                //new Vector2(uvMap.x, uvMap.y),
                //new Vector2(uvMap.z, uvMap.w));
            }

        //Debug.Log("Mesh was redrawn!");
    }

    private int CreateParamShader()
    {
        int result = 0;
        foreach (int colorIndex in _clrIndexes)
        {
            result = result * 100 + colorIndex;
        }

        return result;
    }

    protected void CheckAndCreateNewRegionPos()
    {
        Vector2 bottomLeftCorner = new Vector2(0, 0) - rectTransform.pivot;
        bottomLeftCorner.x *= rectTransform.rect.width;
        bottomLeftCorner.y *= rectTransform.rect.height;

        Rect rect = rectTransform.rect;

        _xPos[0] = 0;
        _xPos[1] = _offset.x;
        _xPos[2] = rect.width - _offset.x;
        _xPos[3] = rect.width;

        _yPos[0] = 0;
        _yPos[1] = _offset.y;
        _yPos[2] = rect.height - _offset.y;
        _yPos[3] = rect.height;

        for (int yIndex = 0; yIndex < 3; yIndex++)
            for (int xIndex = 0; xIndex < 3; xIndex++)
            {
                _regionPos[xIndex, yIndex] = new Vector4(bottomLeftCorner.x + _xPos[xIndex], bottomLeftCorner.y + _yPos[yIndex],
                                                         bottomLeftCorner.x + _xPos[xIndex + 1], bottomLeftCorner.y + _yPos[yIndex + 1]);


            }

    }

    protected void AddCenterQuad(VertexHelper vh)
    {
        Vector4 curRegionPos = _regionPos[1, 1];
        Vector4 uvMap = _uvMapEdge[1, 1];

        AddQuad(vh,
              new Vector2(curRegionPos.x, curRegionPos.y),
              new Vector2(curRegionPos.z, curRegionPos.w),
              new Vector2(uvMap.x, uvMap.y),
              new Vector2(uvMap.z, uvMap.w));
    }

    protected void AddEdgeQuad(VertexHelper vh, int xDirection, int yDirection)
    {
        Vector2Int curIndexInRegionTile = new Vector2Int(1, 1);
        Vector2Int curIndexInMapping = new Vector2Int(1 + xDirection, 1 + yDirection);

        Vector4 curRegionPos = _regionPos[curIndexInMapping.x, curIndexInMapping.y];
        Vector4 uvMap = _uvMapEdge.GetValueFrom2DArrayByIndex(1, 1);


        if (!_tileNextToItem[curIndexInRegionTile.x + xDirection, curIndexInRegionTile.y + yDirection])
        {
            uvMap = _uvMapEdge.GetValueFrom2DArrayByIndex(curIndexInMapping.x, curIndexInMapping.y);
        }

        AddQuad(vh,
              new Vector2(curRegionPos.x, curRegionPos.y),
              new Vector2(curRegionPos.z, curRegionPos.w),
              new Vector2(uvMap.x, uvMap.y),
              new Vector2(uvMap.z, uvMap.w));
    }

    protected void AddCornerQuad(VertexHelper vh, int xDirection, int yDirection)
    {
        Vector2Int curIndexInRegionTile = new Vector2Int(1, 1);
        Vector2Int curIndexInMapping = new Vector2Int(1 + xDirection, 1 + yDirection);

        Vector4 curRegionPos = _regionPos[curIndexInMapping.x, curIndexInMapping.y];
        Vector4 uvMap = _uvMapEdge.GetValueFrom2DArrayByIndex(1, 1);

        int isHorizontalNearby = _tileNextToItem[curIndexInRegionTile.x + xDirection, curIndexInRegionTile.y] ? 1 : 0;
        int isVerticalNearby = _tileNextToItem[curIndexInRegionTile.x, curIndexInRegionTile.y + yDirection] ? 1 : 0;
        int isDiagonalNearby = _tileNextToItem[curIndexInRegionTile.x + xDirection, curIndexInRegionTile.y + yDirection] ? 1 : 0;

        //not nearby any others tile
        if (isVerticalNearby + isHorizontalNearby == 0)
        {
            uvMap = _uvMapEdge.GetValueFrom2DArrayByIndex(curIndexInMapping.x, curIndexInMapping.y);
        }
        //nearby 1 tile
        else if (isVerticalNearby + isHorizontalNearby == 1)
        {
            if (isDiagonalNearby == 0)
            {
                int finalXUVMapIndex = curIndexInMapping.x - (isHorizontalNearby * xDirection);
                int finalYUVMapIndex = curIndexInMapping.y - (isVerticalNearby * yDirection);
                uvMap = _uvMapEdge.GetValueFrom2DArrayByIndex(finalXUVMapIndex, finalYUVMapIndex);
            }
            else
            {
                uvMap = _uvMapInsideCorner.GetValueFrom2DArrayByCenterDirection(xDirection * (1 - 3 * isHorizontalNearby), yDirection * (1 - 3 * isVerticalNearby));
            }

        }
        else if (isVerticalNearby + isHorizontalNearby == 2 && isDiagonalNearby == 0)
        {
            uvMap = _uvMapInsideCorner.GetValueFrom2DArrayByCenterDirection(xDirection, yDirection);

            //Debug.Log($"{xDirection}__{yDirection}   {uvMap.x}__{uvMap.y}   {uvMap.z}__{uvMap.w}");

            //Add another quad on the diagonal direction
            Vector4 addInRegionPos = curRegionPos;
            addInRegionPos += new Vector4(xDirection * _offset.x, yDirection * _offset.y, xDirection * _offset.x, yDirection * _offset.y);
            Vector4 addInUVMap = _uvMapInsideCorner.GetValueFrom2DArrayByCenterDirection(xDirection * 2, yDirection * 2);

            AddQuad(vh,
          new Vector2(addInRegionPos.x, addInRegionPos.y),
          new Vector2(addInRegionPos.z, addInRegionPos.w),
          new Vector2(addInUVMap.x, addInUVMap.y),
          new Vector2(addInUVMap.z, addInUVMap.w));
        }

        AddQuad(vh,
          new Vector2(curRegionPos.x, curRegionPos.y),
          new Vector2(curRegionPos.z, curRegionPos.w),
          new Vector2(uvMap.x, uvMap.y),
          new Vector2(uvMap.z, uvMap.w));
    }




    [SerializeField, HideInInspector]
    private SerializationData serializationData;

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
    }
}

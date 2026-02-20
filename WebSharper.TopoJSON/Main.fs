namespace WebSharper.TopoJSON.Definitions

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    let (=!>) name signature =
        name => signature
        |> Import name "topojson-client"

    let Transform =
        Pattern.Config "Transform" {
            Required = [
                "scale", !|T<float>
                "translate", !|T<float>
            ]
            Optional = []
        }

    let Geometry =
        Interface "Geometry"
    
    let GeometryCollection =
        Pattern.Config "GeometryCollection" {
            Required = [
                "type", T<string>
                "geometries", !|Geometry
            ]
            Optional = []
        }
        |=> Implements [Geometry]

    let Point =
        Pattern.Config "Point" {
            Required  = [
                "type", T<string>
                "coordinates", Type.ArrayOf T<float>
            ]
            Optional  = [
                "id", T<string>
                "properties", T<obj>
                "bbox", Type.ArrayOf T<float>
            ]
        }
        |=> Implements [Geometry]

    let MultiPoint =
        Pattern.Config "MultiPoint" {
            Required  = [
                "type", T<string>
                "coordinates", Type.ArrayOf (Type.ArrayOf T<float>)
            ]
            Optional  = [
                "id", T<string>
                "properties", T<obj>
                "bbox", Type.ArrayOf T<float>
            ]
        }
        |=> Implements [Geometry]

    let LineString =
        Pattern.Config "LineString" {
            Required  = [
                "type", T<string>
                "arcs", Type.ArrayOf T<int>
            ]
            Optional  = [
                "id", T<string>
                "properties", T<obj>
                "bbox", Type.ArrayOf T<float>
            ]
        }
        |=> Implements [Geometry]

    let MultiLineString =
        Pattern.Config "MultiLineString" {
            Required  = [
                "type", T<string>
                "arcs", Type.ArrayOf (Type.ArrayOf T<int>)
            ]
            Optional  = [
                "id", T<string>
                "properties", T<obj>
                "bbox", Type.ArrayOf T<float>
            ]
        }
        |=> Implements [Geometry]

    let Polygon =
        Pattern.Config "Polygon" {
            Required  = [
                "type", T<string>
                "arcs", Type.ArrayOf (Type.ArrayOf T<int>)
            ]
            Optional  = [
                "id", T<string>
                "properties", T<obj>
                "bbox", Type.ArrayOf T<float>
            ]
        }
        |=> Implements [Geometry]

    let MultiPolygon =
        Pattern.Config "MultiPolygon" {
            Required  = [
                "type", T<string>
                "arcs", Type.ArrayOf (Type.ArrayOf (Type.ArrayOf T<int>))
            ]
            Optional  = [
                "id", T<string>
                "properties", T<obj>
                "bbox", Type.ArrayOf T<float>
            ]
        }
        |=> Implements [Geometry]

    let Topology =
        Pattern.Config "Topology" {
            Required = [
                "type", T<string>
                "arcs", !|(!|(!|T<float>))
                "bbox", !|T<float>
                "objects", T<Map<_,_>>.[T<string>, Geometry]
            ]
            Optional = [
                "transform", Transform.Type
            ]
        }

    let Feature =
        Pattern.Config "Feature" {
            Required = [
                "type", T<string>          // always "Feature"
                "geometry", Geometry.Type
                "properties", T<obj>
            ]
            Optional = [
                "id", T<string>           // GeoJSON allows number or string
            ]
        }

    let FeatureCollection =
        Pattern.Config "FeatureCollection" {
            Required = [
                "type", T<string>             // always "FeatureCollection"
                "features", !| Feature
            ]
            Optional = []
        }

    let GeoMultiPolygon =
        Pattern.Config "GeoMultiPolygon" {
            Required = [
                "type", T<string>
                "coordinates", !|(!|(!|T<float>))
            ]
            Optional = []
        }

    let GeoMultiString =
        Pattern.Config "GeoMultiString" {
            Required = [
                "type", T<string>
                "coordinates", !|(!|T<float>)
            ]
            Optional = []
        }

    let TopoJson =
        Class "TopoJSON"
        |+> Static [
                "feature"   =!> Topology * Geometry ^-> Feature + FeatureCollection
                "merge"     =!> Topology * Geometry ^-> GeoMultiPolygon
                "mergeArcs" =!> Topology * !|Geometry ^-> Geometry
                "mesh"      =!> Topology * !?Geometry * !? T<obj * obj -> bool> ^-> GeoMultiString
                "meshArcs"  =!> Topology * !?Geometry * !? T<obj * obj -> bool> ^-> Geometry
                "neighbors" =!> !|Geometry ^-> !|(!|T<float>)

                "bbox"      =!> Topology ^-> Type.ArrayOf T<float>
                "quantize"  =!> Topology * Transform ^-> Topology
                "transform"  =!> Topology * Type.ArrayOf T<float> ^-> Type.ArrayOf T<float>
                "untransform"  =!> Topology * Type.ArrayOf T<float> ^-> Type.ArrayOf T<float>
            ]

    let Assembly =
        Assembly [
            Namespace "WebSharper.TopoJSON" [
                TopoJson
                Topology
                Geometry
                Feature
                FeatureCollection
                GeoMultiString
                GeoMultiPolygon
                GeometryCollection
                Point
                Polygon
                MultiPolygon
                MultiPoint
                LineString
                MultiLineString
                Transform
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()

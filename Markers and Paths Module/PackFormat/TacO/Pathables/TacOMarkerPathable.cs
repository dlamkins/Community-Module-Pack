﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Content;
using Blish_HUD.Pathing.Entities;
using Blish_HUD.Pathing.Format;
using Markers_and_Paths_Module.PackFormat.TacO.Behavior;
using Microsoft.Xna.Framework;

namespace Markers_and_Paths_Module.PackFormat.TacO.Pathables {
    public class TacOMarkerPathable : LoadedMarkerPathable, ITacOPathable {

        private const float DEFAULT_HEIGHTOFFSET = 1.5f;
        private const float DEFAULT_ICONSIZE = 2f;

        private string _type;
        private PathingCategory _category;
        private int _resetLength;
        private bool _autoTrigger;
        private bool _hasCountdown;
        private float _triggerRange;
        private int _tacOBehaviorId;

        private BasicTacOBehavior<ManagedPathable<Marker>, Marker> _tacOBehavior;

        public string Type {
            get => _type;
            set {
                if (SetProperty(ref _type, value)) {
                    _category = _rootCategory.GetOrAddCategoryFromNamespace(_type);
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        public PathingCategory Category {
            get => _category;
            set {
                if (SetProperty(ref _category, value)) {
                    _type = _category.Namespace;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }

        public int ResetLength {
            get => _resetLength;
            set => SetProperty(ref _resetLength, value);
        }

        public bool AutoTrigger {
            get => _autoTrigger;
            set => SetProperty(ref _autoTrigger, value);
        }

        public bool HasCountdown {
            get => _hasCountdown;
            set => SetProperty(ref _hasCountdown, value);
        }
        public float HeightOffset {
            get => this.ManagedEntity.VerticalOffset;
            set { this.ManagedEntity.VerticalOffset = value; OnPropertyChanged(); }
        }

        public float TriggerRange {
            get => _triggerRange;
            set => SetProperty(ref _triggerRange, value);
        }

        public int TacOBehaviorId {
            get => _tacOBehaviorId;
            set {
                if (SetProperty(ref _tacOBehaviorId, value)) {
                    //this.Behavior.Remove(_tacOBehavior);

                    //_tacOBehavior = new BasicTacOBehavior<ManagedPathable<Marker>, Marker>(this, (TacOBehavior)_tacOBehaviorId);

                    //this.Behavior.Add(_tacOBehavior);
                }
            }
        }

        private readonly XmlNode _sourceNode;
        private readonly PathingCategory _rootCategory;

        public TacOMarkerPathable(XmlNode sourceNode, PathableResourceManager packContext, PathingCategory rootCategory) : base(packContext) {
            _sourceNode = sourceNode;
            _rootCategory = rootCategory;

            BeginLoad();
        }

        // TODO: Use this method as an opportunity to convert attributes to some sort of IPathingAttribute to keep things
        // consistent between imported file formats
        protected override void BeginLoad() {
            LoadAttributes(_sourceNode);
        }

        protected override void PrepareAttributes() {
            // Type
            RegisterAttribute("type", attribute => (!string.IsNullOrEmpty(this.Type = attribute.Value.Trim())));

            // Alpha (alias:Opacity)
            RegisterAttribute("alpha", delegate (XmlAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.Opacity = fOut;
                return true;
            });

            // FadeNear
            RegisterAttribute("fadeNear", delegate (XmlAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.FadeNear = fOut;
                return true;
            });

            // FadeFar
            RegisterAttribute("fadeFar", delegate (XmlAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.FadeFar = fOut;
                return true;
            });

            // IconSize
            RegisterAttribute("iconSize", delegate (XmlAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.AutoResize = false;
                this.ManagedEntity.Size = new Vector2(fOut * 2f);
                return true;
            });

            // HeightOffset
            RegisterAttribute("heightOffset", delegate (XmlAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.HeightOffset = fOut;
                return true;
            });

            // ResetLength
            RegisterAttribute("resetLength", delegate (XmlAttribute attribute) {
                if (!InvariantUtil.TryParseInt(attribute.Value, out int iOut)) return false;

                this.ResetLength = iOut;
                return true;
            });

            // AutoTrigger
            RegisterAttribute("autoTrigger", delegate (XmlAttribute attribute) {
                this.AutoTrigger = (attribute.Value == "0");
                return true;
            });

            // AutoTrigger
            RegisterAttribute("hasCountdown", delegate (XmlAttribute attribute) {
                this.HasCountdown = (attribute.Value == "0");
                return true;
            });

            // TriggerRange
            RegisterAttribute("triggerRange", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float fOut)) return false;

                this.TriggerRange = fOut;
                return true;
            });

            // Taco Behavior
            RegisterAttribute("behavior", delegate (XmlAttribute attribute) {
                if (!int.TryParse(attribute.Value, out int iOut)) return false;

                this.TacOBehaviorId = iOut;
                return true;
            });

            base.PrepareAttributes();
        }

        protected override bool FinalizeAttributes(Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders) {
            // Process attributes from type category first
            if (_category?.SourceXmlNode?.Attributes != null) {
                ProcessAttributes(_category.SourceXmlNode.Attributes);
            }

            _category?.AddPathable(this);

            // Finalize attributes
            if (attributeLoaders.ContainsKey("heightoffset")) {
                if (!attributeLoaders["heightoffset"].Loaded) {
                    this.HeightOffset = DEFAULT_HEIGHTOFFSET;
                    this.ManagedEntity.VerticalConstraint = BillboardVerticalConstraint.CameraPosition;
                }
            }
            if (attributeLoaders.ContainsKey("iconsize")) {
                if (!attributeLoaders["iconsize"].Loaded) {
                    this.ManagedEntity.Size = new Vector2(DEFAULT_ICONSIZE);
                }
            }

            // Let base finalize attributes
            return base.FinalizeAttributes(attributeLoaders);
        }

    }
}

// Copyright (c) 2017 Enflux Inc.
// By downloading, accessing or using this SDK, you signify that you have read, understood and agree to the terms and conditions of the End User License Agreement located at: https://www.getenflux.com/pages/sdk-eula

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Enflux.SDK.Utils
{
    public static class RigUtils
    {
        private static readonly string[] StandardHeadNames =
{
            "head"
        };

        private static readonly string[] StandardNeckNames =
        {
            "neck"
        };

        private static readonly string[] StandardCollarNames =
        {
            "collar"
        };

        private static readonly string[] StandardCoreNames =
        {
            "core"
        };

        private static readonly string[] StandardChestNames =
        {
            "chest"
        };

        private static readonly string[] StandardUpperArmNames =
        {
            "shldr",
            "shoulder"
        };

        private static readonly string[] StandardLowerArmNames =
        {
            "forearm",
            "elbow"
        };

        private static readonly string[] StandardHandNames =
        {
            "hand",
        };

        private static readonly string[] StandardThumbProximalNames =
        {
            "thumb1",
            "thmb1",
        };

        private static readonly string[] StandardThumbIntermediateNames =
        {
            "thumb2",
            "thmb2",
        };

        private static readonly string[] StandardThumbDistalNames =
        {
            "thumb3",
            "thmb3",
        };

        private static readonly string[] StandardIndexProximalNames =
        {
            "index1",
            "indx1",
            "idx1",
        };

        private static readonly string[] StandardIndexIntermediateNames =
        {
            "index2",
            "indx2",
            "idx2",
        };

        private static readonly string[] StandardIndexDistalNames =
        {
            "index3",
            "indx3",
            "idx3",
        };

        private static readonly string[] StandardMiddleProximalNames =
        {
            "middle1",
            "mid1",
        };

        private static readonly string[] StandardMiddleIntermediateNames =
        {
            "middle2",
            "mid2",
        };

        private static readonly string[] StandardMiddleDistalNames =
        {
            "middle3",
            "mid3",
        };

        private static readonly string[] StandardRingProximalNames =
        {
            "ring1",
            "rng1",
        };

        private static readonly string[] StandardRingIntermediateNames =
        {
            "ring2",
            "rng2",
        };

        private static readonly string[] StandardRingDistalNames =
        {
            "ring3",
            "rng3",
        };


        private static readonly string[] StandardPinkyProximalNames =
        {
            "pinky1",
            "little1",
            "pnky1",
            "lit1",
            "pnk1",
        };

        private static readonly string[] StandardPinkyIntermediateNames =
        {
            "pinky2",
            "little2",
            "pnky2",
            "lit2",
            "pnk2",
        };

        private static readonly string[] StandardPinkyDistalNames =
        {
            "pinky3",
            "little3",
            "pnky3",
            "lit3",
            "pnk3",
        };

        private static readonly string[] StandardWaistNames =
        {
            "waist"
        };

        private static readonly string[] StandardUpperLegNames =
        {
            "thigh",
            "leg"
        };

        private static readonly string[] StandardLowerLegNames =
        {
            "shin",
            "knee"
        };

        private static readonly string[] StandardFootNames =
        {
            "foot"
        };

        private static readonly string[] StandardToeNames =
        {
            "toe"
        };

        private static readonly string[] StandardLeftNames =
        {
            "l",
            "left",
            "lft",
            "lt"
        };

        private static readonly string[] StandardRightNames =
        {
            "r",
            "right",
            "rgt",
            "rt"
        };

        private static readonly string[] StandardCenterNames =
        {
            "",
            "center",
            "cen"
        };

        public static Transform ResolveHead(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardHeadNames, StandardCenterNames);
        }

        public static Transform ResolveNeck(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardNeckNames, StandardCenterNames);
        }

        public static Transform ResolveCore(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardCoreNames, StandardCenterNames);
        }

        public static Transform ResolveChest(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardChestNames, StandardCenterNames);
        }

        public static Transform ResolveLeftCollar(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardCollarNames, StandardLeftNames);
        }

        public static Transform ResolveRightCollar(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardCollarNames, StandardRightNames);
        }

        public static Transform ResolveLeftUpperArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperArmNames, StandardLeftNames);
        }

        public static Transform ResolveLeftLowerArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerArmNames, StandardLeftNames);
        }

        public static Transform ResolveLeftHand(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardHandNames, StandardLeftNames);
        }

        public static Transform ResolveLeftThumbProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardThumbProximalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftThumbIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardThumbIntermediateNames, StandardLeftNames);
        }

        public static Transform ResolveLeftThumbDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardThumbDistalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftIndexProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardIndexProximalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftIndexIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardIndexIntermediateNames, StandardLeftNames);
        }

        public static Transform ResolveLeftIndexDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardIndexDistalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftMiddleProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardMiddleProximalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftMiddleIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardMiddleIntermediateNames, StandardLeftNames);
        }

        public static Transform ResolveLeftMiddleDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardMiddleDistalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftRingProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardRingProximalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftRingIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardRingIntermediateNames, StandardLeftNames);
        }

        public static Transform ResolveLeftRingDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardRingDistalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftPinkyProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardPinkyProximalNames, StandardLeftNames);
        }

        public static Transform ResolveLeftPinkyIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardPinkyIntermediateNames, StandardLeftNames);
        }

        public static Transform ResolveLeftPinkyDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardPinkyDistalNames, StandardLeftNames);
        }

        public static Transform ResolveRightUpperArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperArmNames, StandardRightNames);
        }

        public static Transform ResolveRightLowerArm(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerArmNames, StandardRightNames);
        }

        public static Transform ResolveRightHand(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardHandNames, StandardRightNames);
        }

        public static Transform ResolveRightThumbProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardThumbProximalNames, StandardRightNames);
        }

        public static Transform ResolveRightThumbIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardThumbIntermediateNames, StandardRightNames);
        }

        public static Transform ResolveRightThumbDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardThumbDistalNames, StandardRightNames);
        }

        public static Transform ResolveRightIndexProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardIndexProximalNames, StandardRightNames);
        }

        public static Transform ResolveRightIndexIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardIndexIntermediateNames, StandardRightNames);
        }

        public static Transform ResolveRightIndexDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardIndexDistalNames, StandardRightNames);
        }

        public static Transform ResolveRightMiddleProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardMiddleProximalNames, StandardRightNames);
        }

        public static Transform ResolveRightMiddleIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardMiddleIntermediateNames, StandardRightNames);
        }

        public static Transform ResolveRightMiddleDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardMiddleDistalNames, StandardRightNames);
        }

        public static Transform ResolveRightRingProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardRingProximalNames, StandardRightNames);
        }

        public static Transform ResolveRightRingIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardRingIntermediateNames, StandardRightNames);
        }

        public static Transform ResolveRightRingDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardRingDistalNames, StandardRightNames);
        }

        public static Transform ResolveRightPinkyProximal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardPinkyProximalNames, StandardRightNames);
        }

        public static Transform ResolveRightPinkyIntermediate(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardPinkyIntermediateNames, StandardRightNames);
        }

        public static Transform ResolveRightPinkyDistal(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardPinkyDistalNames, StandardRightNames);
        }

        public static Transform ResolveWaist(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardWaistNames, StandardCenterNames);
        }

        public static Transform ResolveLeftUpperLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperLegNames, StandardLeftNames);
        }

        public static Transform ResolveLeftLowerLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerLegNames, StandardLeftNames);
        }

        public static Transform ResolveLeftFoot(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardFootNames, StandardLeftNames);
        }

        public static Transform ResolveLeftToe(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardToeNames, StandardLeftNames);
        }

        public static Transform ResolveRightUpperLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardUpperLegNames, StandardRightNames);
        }

        public static Transform ResolveRightLowerLeg(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardLowerLegNames, StandardRightNames);
        }

        public static Transform ResolveRightFoot(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardFootNames, StandardRightNames);
        }

        public static Transform ResolveRightToe(Transform root)
        {
            return ResolveLimbTransformByName(root, StandardToeNames, StandardRightNames);
        }

        /// <summary>
        /// Returns a child transform by taking a list of possible names, a list of possible prefixes/suffixes, and checking if the child transform's name matches a possible pattern. Allows a hypens, spaces, and underscores between the limb name and prefix/suffix. Doesn't check if capitalization is the same.
        ///
        /// Note that this method is not performant, as it greedily allocates memory as needed for checking each transform. 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="possibleLimbNames"></param>
        /// <param name="possibleDirectionNames">List of possible prefixes or suffixes</param>
        /// <returns></returns>
        private static Transform ResolveLimbTransformByName(this Transform root, IList<string> possibleLimbNames, IList<string> possibleDirectionNames)
        {
            if (root == null)
            {
                return null;
            }
            // TODO: Can probably change the ordering of loops/checks to significantly optimize the speed of this method! 
            const string regexFormatString = "{0}[\\ _\\-]*{1}";
            var children = root.GetComponentsInChildren<Transform>(true);
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < children.Length; ++i)
            {
                var childName = children[i].name.ToLower();
                // ReSharper disable once LoopCanBeConvertedToQuery
                // ReSharper disable once ForCanBeConvertedToForeach
                for (var j = 0; j < possibleLimbNames.Count; ++j)
                {
                    var limbName = possibleLimbNames[j];
                    // ReSharper disable once LoopCanBeConvertedToQuery
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var k = 0; k < possibleDirectionNames.Count; ++k)
                    {
                        var directionName = possibleDirectionNames[k];

                        // Check for directiom name then "", " ", "_", or "-" then limb name.
                        var prefixRegex = new Regex(string.Format(regexFormatString, directionName, limbName));
                        if (prefixRegex.IsMatch(childName))
                        {
                            return children[i];
                        }
                        // Check for limb name then "", " ", "_", or "-" then direction name.
                        var suffixRegex = new Regex(string.Format(regexFormatString, limbName, directionName));
                        if (suffixRegex.IsMatch(childName))
                        {
                            return children[i];
                        }
                    }
                }
            }
            return null;
        }
    }
}
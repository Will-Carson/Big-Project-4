//#templateid: Stats.MyCustomTypeTemplate
//#region __GHOST_IMPORTS__
//using Unity.Collections.LowLevel.Unsafe;
//using Unity.Networking.Transport;
//using Unity.Mathematics;
//using Unity.Entities;
//using Unity.Collections;
//using Unity.NetCode;
//#endregion
//namespace Generated
//{
//    public struct GhostSnapshotData
//    {
//        struct Snapshot
//        {
//            #region __GHOST_FIELD__
//            public UnsafeHashMap<uint, float> __GHOST_FIELD_NAME__;
//            #endregion
//        }

//        public void PredictDelta(uint tick, ref GhostSnapshotData baseline1, ref GhostSnapshotData baseline2)
//        {
//            var predictor = new GhostDeltaPredictor(new NetworkTick(tick), this.tick, baseline1.tick, baseline2.tick);
//            #region __GHOST_PREDICT__
//            snapshot.__GHOST_FIELD_NAME__ = predictor.PredictHashMap(snapshot.__GHOST_FIELD_NAME__, baseline1.__GHOST_FIELD_NAME__, baseline2.__GHOST_FIELD_NAME__);
//            #endregion
//        }

//        public void Serialize(int networkId, ref GhostSnapshotData baseline, ref DataStreamWriter writer, StreamCompressionModel compressionModel)
//        {
//            #region __GHOST_WRITE__
//            if ((changeMask & (1 << __GHOST_MASK_INDEX__)) != 0)
//            {
//                var changeBuffer = new NativeList<DiffHashMapChange<uint, float>>(Allocator.Temp);
//                UnsafeHashMap<uint, float>.Diff(baseline.__GHOST_FIELD_NAME__, snapshot.__GHOST_FIELD_NAME__, changeBuffer);
//                writer.WritePackedInt(changeBuffer.Length);
//                for (var i = 0; i < changeBuffer.Length; i++)
//                {
//                    writer.WritePackedUIntDelta(changeBuffer[i].Key, 0, compressionModel);
//                    writer.WriteFloatDelta(changeBuffer[i].Value, 0.0f, compressionModel);
//                }
//                changeBuffer.Dispose();
//            }
//            #endregion
//        }

//        public void Deserialize(uint tick, ref GhostSnapshotData baseline, ref DataStreamReader reader,
//            StreamCompressionModel compressionModel)
//        {
//            #region __GHOST_READ__
//            if ((changeMask & (1 << __GHOST_MASK_INDEX__)) != 0)
//            {
//                var changeCount = reader.ReadPackedInt();
//                for (var i = 0; i < changeCount; i++)
//                {
//                    var key = reader.ReadPackedUIntDelta(0, compressionModel);
//                    var value = reader.ReadFloatDelta(0.0f, compressionModel);
//                    snapshot.__GHOST_FIELD_NAME__[key] = value;
//                }
//            }
//            else
//            {
//                snapshot.__GHOST_FIELD_NAME__.Clear();
//                foreach (var kvp in baseline.__GHOST_FIELD_NAME__)
//                    snapshot.__GHOST_FIELD_NAME__.Add(kvp.Key, kvp.Value);
//            }
//            #endregion
//        }

//        public unsafe void CopyToSnapshot(ref Snapshot snapshot, ref IComponentData component)
//        {
//            if (true)
//            {
//                #region __GHOST_COPY_TO_SNAPSHOT__
//                snapshot.__GHOST_FIELD_NAME__.Clear();
//                foreach (var kvp in component.__GHOST_FIELD_REFERENCE__)
//                    snapshot.__GHOST_FIELD_NAME__[kvp.Key] = kvp.Value;
//                #endregion
//            }
//        }

//        public unsafe void CopyFromSnapshot(ref Snapshot snapshot, ref IComponentData component)
//        {
//            if (true)
//            {
//                #region __GHOST_COPY_FROM_SNAPSHOT__
//                component.__GHOST_FIELD_REFERENCE__.Clear();
//                foreach (var kvp in snapshotBefore.__GHOST_FIELD_NAME__)
//                    component.__GHOST_FIELD_REFERENCE__[kvp.Key] = kvp.Value;
//                #endregion

//                #region __GHOST_COPY_FROM_SNAPSHOT_INTERPOLATE_SETUP__
//                var __GHOST_FIELD_NAME___Before = snapshotBefore.__GHOST_FIELD_NAME__;
//                var __GHOST_FIELD_NAME___After = snapshotAfter.__GHOST_FIELD_NAME__;
//                #endregion
//                #region __GHOST_COPY_FROM_SNAPSHOT_INTERPOLATE_DISTSQ__
//                var __GHOST_FIELD_NAME___DistSq = math.distancesq(__GHOST_FIELD_NAME___Before.Length, __GHOST_FIELD_NAME___After.Length);
//                #endregion
//                #region __GHOST_COPY_FROM_SNAPSHOT_INTERPOLATE__
//                var lerpFactor = math.clamp(snapshotInterpolationFactor, 0.0f, 1.0f);
//                component.__GHOST_FIELD_REFERENCE__.Clear();
//                foreach (var kvpBefore in __GHOST_FIELD_NAME___Before)
//                {
//                    float valueAfter;
//                    if (__GHOST_FIELD_NAME___After.TryGetValue(kvpBefore.Key, out valueAfter))
//                    {
//                        var lerpValue = math.lerp(kvpBefore.Value, valueAfter, lerpFactor);
//                        component.__GHOST_FIELD_REFERENCE__[kvpBefore.Key] = lerpValue;
//                    }
//                    else
//                    {
//                        component.__GHOST_FIELD_REFERENCE__[kvpBefore.Key] = kvpBefore.Value;
//                    }
//                }
//                foreach (var kvpAfter in __GHOST_FIELD_NAME___After)
//                {
//                    if (!__GHOST_FIELD_NAME___Before.ContainsKey(kvpAfter.Key))
//                        component.__GHOST_FIELD_REFERENCE__[kvpAfter.Key] = kvpAfter.Value;
//                }
//                #endregion
//            }
//        }

//        public unsafe void RestoreFromBackup(ref IComponentData component, in IComponentData backup)
//        {
//            #region __GHOST_RESTORE_FROM_BACKUP__
//            component.__GHOST_FIELD_REFERENCE__.Clear();
//            foreach (var kvp in backup.__GHOST_FIELD_REFERENCE__)
//                component.__GHOST_FIELD_REFERENCE__[kvp.Key] = kvp.Value;
//            #endregion
//        }

//        public void CalculateChangeMask(ref Snapshot snapshot, ref Snapshot baseline, uint changeMask)
//        {
//            #region __GHOST_CALCULATE_CHANGE_MASK_ZERO__
//            changeMask = (snapshot.__GHOST_FIELD_NAME__.Count != baseline.__GHOST_FIELD_NAME__.Count) ? 1u : 0;
//            #endregion
//            #region __GHOST_CALCULATE_CHANGE_MASK__
//            changeMask |= (snapshot.__GHOST_FIELD_NAME__.Count != baseline.__GHOST_FIELD_NAME__.Count) ? (1u << __GHOST_MASK_INDEX__) : 0;
//            #endregion
//        }

//#if UNITY_EDITOR || NETCODE_DEBUG
//        private static void ReportPredictionErrors(ref IComponentData component, in IComponentData backup, ref UnsafeList<float> errors, ref int errorIndex)
//        {
//            #region __GHOST_REPORT_PREDICTION_ERROR__
//            // Custom error reporting for UnsafeHashMap type
//            foreach (var kvp in component.__GHOST_FIELD_REFERENCE__)
//            {
//                float backupValue;
//                if (backup.__GHOST_FIELD_REFERENCE__.TryGetValue(kvp.Key, out backupValue))
//                {
//                    var diff = math.abs(kvp.Value - backupValue);
//                    errors[errorIndex] = math.max(errors[errorIndex], diff);
//                    ++errorIndex;
//                }
//            }
//            #endregion
//        }

//        private static int GetPredictionErrorNames(ref FixedString512Bytes names, ref int nameCount)
//        {
//            #region __GHOST_GET_PREDICTION_ERROR_NAME__
//            if (nameCount != 0)
//                names.Append(new FixedString32Bytes(","));
//            names.Append(new FixedString64Bytes("__GHOST_FIELD_REFERENCE__"));
//            ++nameCount;
//            #endregion
//        }
//#endif
//    }
//}

////#templateid: Stats.MyCustomTypeTemplate
////#region __GHOST_IMPORTS__
////#endregion
////namespace Generated
////{
////    public struct GhostSnapshotData
////    {
////        struct Snapshot
////        {
////            #region __GHOST_FIELD__
////            public int __GHOST_FIELD_NAME__;
////            #endregion
////        }

////        public void PredictDelta(uint tick, ref GhostSnapshotData baseline1, ref GhostSnapshotData baseline2)
////        {
////            var predictor = new GhostDeltaPredictor(new NetworkTick(tick), this.tick, baseline1.tick, baseline2.tick);
////            #region __GHOST_PREDICT__
////            snapshot.__GHOST_FIELD_NAME__ = predictor.PredictInt(snapshot.__GHOST_FIELD_NAME__, baseline1.__GHOST_FIELD_NAME__, baseline2.__GHOST_FIELD_NAME__);
////            #endregion
////        }

////        public void Serialize(int networkId, ref GhostSnapshotData baseline, ref DataStreamWriter writer, StreamCompressionModel compressionModel)
////        {
////            #region __GHOST_WRITE__
////            if ((changeMask & (1 << __GHOST_MASK_INDEX__)) != 0)
////                writer.WritePackedIntDelta(snapshot.__GHOST_FIELD_NAME__, baseline.__GHOST_FIELD_NAME__, compressionModel);
////            #endregion
////        }

////        public void Deserialize(uint tick, ref GhostSnapshotData baseline, ref DataStreamReader reader,
////            StreamCompressionModel compressionModel)
////        {
////            #region __GHOST_READ__
////            if ((changeMask & (1 << __GHOST_MASK_INDEX__)) != 0)
////                snapshot.__GHOST_FIELD_NAME__ = reader.ReadPackedIntDelta(baseline.__GHOST_FIELD_NAME__, compressionModel);
////            else
////                snapshot.__GHOST_FIELD_NAME__ = baseline.__GHOST_FIELD_NAME__;
////            #endregion
////        }

////        public unsafe void CopyToSnapshot(ref Snapshot snapshot, ref IComponentData component)
////        {
////            if (true)
////            {
////                #region __GHOST_COPY_TO_SNAPSHOT__
////                snapshot.__GHOST_FIELD_NAME__ = (int) math.round(component.__GHOST_FIELD_REFERENCE__ * __GHOST_QUANTIZE_SCALE__);
////                #endregion
////            }
////        }
////        public unsafe void CopyFromSnapshot(ref Snapshot snapshot, ref IComponentData component)
////        {
////            if (true)
////            {
////                #region __GHOST_COPY_FROM_SNAPSHOT__
////                component.__GHOST_FIELD_REFERENCE__ = snapshotBefore.__GHOST_FIELD_NAME__ * __GHOST_DEQUANTIZE_SCALE__;
////                #endregion

////                #region __GHOST_COPY_FROM_SNAPSHOT_INTERPOLATE_SETUP__
////                var __GHOST_FIELD_NAME___Before = snapshotBefore.__GHOST_FIELD_NAME__ * __GHOST_DEQUANTIZE_SCALE__;
////                var __GHOST_FIELD_NAME___After = snapshotAfter.__GHOST_FIELD_NAME__ * __GHOST_DEQUANTIZE_SCALE__;
////                #endregion
////                #region __GHOST_COPY_FROM_SNAPSHOT_INTERPOLATE_DISTSQ__
////                var __GHOST_FIELD_NAME___DistSq = math.distancesq(__GHOST_FIELD_NAME___Before, __GHOST_FIELD_NAME___After);
////                #endregion
////                #region __GHOST_COPY_FROM_SNAPSHOT_INTERPOLATE__
////                component.__GHOST_FIELD_REFERENCE__ = math.lerp(__GHOST_FIELD_NAME___Before, __GHOST_FIELD_NAME___After, snapshotInterpolationFactor);
////                #endregion
////            }
////        }
////        public unsafe void RestoreFromBackup(ref IComponentData component, in IComponentData backup)
////        {
////            #region __GHOST_RESTORE_FROM_BACKUP__
////            component.__GHOST_FIELD_REFERENCE__ = backup.__GHOST_FIELD_REFERENCE__;
////            #endregion
////        }
////        public void CalculateChangeMask(ref Snapshot snapshot, ref Snapshot baseline, uint changeMask)
////        {
////            #region __GHOST_CALCULATE_CHANGE_MASK_ZERO__
////            changeMask = (snapshot.__GHOST_FIELD_NAME__ != baseline.__GHOST_FIELD_NAME__) ? 1u : 0;
////            #endregion
////            #region __GHOST_CALCULATE_CHANGE_MASK__
////            changeMask |= (snapshot.__GHOST_FIELD_NAME__ != baseline.__GHOST_FIELD_NAME__) ? (1u<<__GHOST_MASK_INDEX__) : 0;
////            #endregion
////        }
////        #if UNITY_EDITOR || NETCODE_DEBUG
////        private static void ReportPredictionErrors(ref IComponentData component, in IComponentData backup, ref UnsafeList<float> errors, ref int errorIndex)
////        {
////            #region __GHOST_REPORT_PREDICTION_ERROR__
////            errors[errorIndex] = math.max(errors[errorIndex], math.abs(component.__GHOST_FIELD_REFERENCE__ - backup.__GHOST_FIELD_REFERENCE__));
////            ++errorIndex;
////            #endregion
////        }
////        private static int GetPredictionErrorNames(ref FixedString512Bytes names, ref int nameCount)
////        {
////            #region __GHOST_GET_PREDICTION_ERROR_NAME__
////            if (nameCount != 0)
////                names.Append(new FixedString32Bytes(","));
////            names.Append(new FixedString64Bytes("__GHOST_FIELD_REFERENCE__"));
////            ++nameCount;
////            #endregion
////        }
////        #endif
////    }
////}
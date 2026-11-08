using System.Collections.Generic;
using Roughtale8.CameraSystem;
using Roughtale8.Core;
using Roughtale8.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Roughtale8.Environment
{
    public sealed class CityGreyboxBuilder : MonoBehaviour
    {
        private Material asphaltMaterial;
        private Material sidewalkMaterial;
        private Material buildingConcreteMaterial;
        private Material buildingAccentMaterial;
        private Material windowGlowMaterial;
        private Material monumentBronzeMaterial;
        private Material foliageMaterial;
        private Material lampGoldMaterial;
        private Material barrierWarningMaterial;
        private Material alleyDarkMaterial;

        private GameObject playerObject;
        private EidonPlayerController playerController;
        private Light flickeringAlleyLight;

        private readonly List<InteractiveAnchor> interactiveAnchors = new List<InteractiveAnchor>();
        private InteractiveAnchor activePromptAnchor;
        private string activeDialogTitle = string.Empty;
        private string activeDialogText = string.Empty;
        private bool isDialogOpen;

        private struct InteractiveAnchor
        {
            public Vector3 Position;
            public float Radius;
            public string Prompt;
            public string Title;
            public string Description;
        }

        private void Awake()
        {
            CreateMaterials();
            SetupAtmosphereAndLighting();
            CreateGroundAndRoadNetwork();
            CreateSidewalks();
            CreateMonumentPlaza();
            CreateCommercialAndResidentialBlocks();
            CreateTheAlleyway();
            CreateNorthPerimeterGate();
            CreateSkylineBackdrop();
            CreateStreetFurnitureAndProps();
            CreatePlayerAndCamera();
            RegisterInteractionPoints();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            // Animate flickering alley light
            if (flickeringAlleyLight != null)
            {
                flickeringAlleyLight.intensity = 1.2f + Mathf.Sin(Time.time * 24f) * 0.6f + Random.Range(-0.25f, 0.25f);
            }

            // Handle Dialog Closing
            if (isDialogOpen)
            {
                if (keyboard.spaceKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
                {
                    CloseDialog();
                }
                return;
            }

            // Quick Hotkeys
            if (keyboard.rKey.wasPressedThisFrame)
            {
                ApplicationFlow.RestartVerticalSlice();
                return;
            }
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                ApplicationFlow.ReturnToMainMenu();
                return;
            }

            // Check Interactive Anchors Distance
            UpdateInteractionDetection();

            // Trigger Interaction
            if (activePromptAnchor.Prompt != null && keyboard.eKey.wasPressedThisFrame)
            {
                OpenDialog(activePromptAnchor.Title, activePromptAnchor.Description);
            }
        }

        private void UpdateInteractionDetection()
        {
            if (playerObject == null) return;

            var playerPos = playerObject.transform.position;
            InteractiveAnchor? closest = null;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < interactiveAnchors.Count; i++)
            {
                var anchor = interactiveAnchors[i];
                var distance = Vector3.Distance(playerPos, anchor.Position);
                if (distance <= anchor.Radius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = anchor;
                }
            }

            activePromptAnchor = closest ?? default;
        }

        private void OpenDialog(string title, string text)
        {
            isDialogOpen = true;
            activeDialogTitle = title;
            activeDialogText = text;
            if (playerController != null)
            {
                playerController.SetMovementLocked(true);
            }
        }

        private void CloseDialog()
        {
            isDialogOpen = false;
            activeDialogTitle = string.Empty;
            activeDialogText = string.Empty;
            if (playerController != null)
            {
                playerController.SetMovementLocked(false);
            }
        }

        private void OnGUI()
        {
            var screenW = Screen.width;
            var screenH = Screen.height;

            // 1. Top Header Banner
            var headerStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            GUI.backgroundColor = new Color(0.08f, 0.10f, 0.12f, 0.85f);
            GUI.contentColor = new Color(0.92f, 0.94f, 0.96f);
            GUI.Box(new Rect(screenW * 0.5f - 260f, 16f, 520f, 32f), "ROUGHTALE8 // WORLD VIII, DAY 01 — 08:17 AM", headerStyle);

            // 2. Dynamic Zone Indicator
            var zoneName = DetermineCurrentZone();
            var zoneStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Normal
            };
            GUI.contentColor = new Color(0.72f, 0.78f, 0.85f);
            GUI.Label(new Rect(0f, 52f, screenW, 22f), zoneName, zoneStyle);

            // 3. Interaction Prompt Floating at Lower Center
            if (!isDialogOpen && !string.IsNullOrEmpty(activePromptAnchor.Prompt))
            {
                var promptBoxStyle = new GUIStyle(GUI.skin.box)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 13,
                    fontStyle = FontStyle.Bold
                };
                GUI.backgroundColor = new Color(0.12f, 0.15f, 0.18f, 0.92f);
                GUI.contentColor = new Color(1f, 0.88f, 0.52f);
                GUI.Box(new Rect(screenW * 0.5f - 210f, screenH - 120f, 420f, 38f), $"[E]  {activePromptAnchor.Prompt}", promptBoxStyle);
            }

            // 4. Modal Dialog / Narrative Inspection Window
            if (isDialogOpen)
            {
                GUI.backgroundColor = new Color(0.06f, 0.08f, 0.10f, 0.96f);
                var dialogWidth = Mathf.Min(680f, screenW - 60f);
                var dialogHeight = 220f;
                var dialogRect = new Rect((screenW - dialogWidth) * 0.5f, (screenH - dialogHeight) * 0.5f, dialogWidth, dialogHeight);
                GUI.Box(dialogRect, GUIContent.none);

                var titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
                GUI.contentColor = new Color(1f, 0.82f, 0.45f);
                GUI.Label(new Rect(dialogRect.x + 24f, dialogRect.y + 20f, dialogWidth - 48f, 28f), activeDialogTitle, titleStyle);

                var descStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
                GUI.contentColor = new Color(0.90f, 0.92f, 0.94f);
                GUI.Label(new Rect(dialogRect.x + 24f, dialogRect.y + 56f, dialogWidth - 48f, 110f), activeDialogText, descStyle);

                var dismissStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.LowerRight,
                    fontStyle = FontStyle.Italic
                };
                GUI.contentColor = new Color(0.60f, 0.65f, 0.70f);
                GUI.Label(new Rect(dialogRect.x + 24f, dialogRect.y + dialogHeight - 34f, dialogWidth - 48f, 22f), "[ Tekan SPACE atau E untuk Menutup ]", dismissStyle);
            }

            // 5. Bottom Navigation Helper
            var footerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11
            };
            GUI.contentColor = new Color(0.55f, 0.60f, 0.66f);
            GUI.Label(new Rect(0f, screenH - 34f, screenW, 24f), "WASD: Gerak  |  Shift: Jog  |  Mouse: Kamera  |  E: Periksa  |  R: Restart  |  Esc: Menu", footerStyle);
        }

        private string DetermineCurrentZone()
        {
            if (playerObject == null) return "DISTRICT 8 — SECTOR CENTRAL";
            var pos = playerObject.transform.position;

            if (pos.x < -18f && pos.z >= 20f && pos.z <= 46f)
            {
                return "⚠ SECTOR 07 : MAINTENANCE ALLEYWAY // ANOMALY DETECTED";
            }
            if (pos.z > 58f)
            {
                return "DISTRICT 8 : NORTH PERIMETER & GATE 8 CHECKPOINT";
            }
            if (pos.z >= -12f && pos.z <= 24f)
            {
                return "DISTRICT 8 : THE MONUMENT PLAZA";
            }
            return "DISTRICT 8 : CENTRAL PROMENADE & RESIDENTIAL QUARTER";
        }

        private void CreateMaterials()
        {
            asphaltMaterial = CreateMaterial(new Color(0.12f, 0.13f, 0.14f));
            sidewalkMaterial = CreateMaterial(new Color(0.48f, 0.50f, 0.52f));
            buildingConcreteMaterial = CreateMaterial(new Color(0.24f, 0.27f, 0.30f));
            buildingAccentMaterial = CreateMaterial(new Color(0.18f, 0.20f, 0.22f));
            windowGlowMaterial = CreateMaterial(new Color(0.75f, 0.85f, 0.95f), new Color(0.20f, 0.35f, 0.50f));
            monumentBronzeMaterial = CreateMaterial(new Color(0.68f, 0.52f, 0.28f));
            foliageMaterial = CreateMaterial(new Color(0.24f, 0.38f, 0.26f));
            lampGoldMaterial = CreateMaterial(new Color(1f, 0.86f, 0.50f), new Color(0.85f, 0.65f, 0.25f));
            barrierWarningMaterial = CreateMaterial(new Color(0.65f, 0.20f, 0.18f));
            alleyDarkMaterial = CreateMaterial(new Color(0.08f, 0.09f, 0.10f));
        }

        private static Material CreateMaterial(Color baseColor, Color? emissionColor = null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = baseColor;
            if (emissionColor.HasValue)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissionColor.Value);
            }
            return mat;
        }

        private static void SetupAtmosphereAndLighting()
        {
            // Golden Morning Sun (08:17 AM)
            var lightObj = new GameObject("Morning Sun");
            var sun = lightObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1.0f, 0.94f, 0.86f);
            lightObj.transform.rotation = Quaternion.Euler(38f, -35f, 0f);

            // Sky Ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.36f, 0.42f, 0.50f);

            // Atmospheric Fog
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.64f, 0.70f, 0.78f);
            RenderSettings.fogDensity = 0.0075f;
        }

        private void CreateGroundAndRoadNetwork()
        {
            // Ground Foundation Plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "City Ground Master";
            ground.transform.position = new Vector3(0f, -0.05f, 20f);
            ground.transform.localScale = new Vector3(16f, 1f, 24f);
            ground.GetComponent<Renderer>().material = asphaltMaterial;

            // Main Central Street Avenue (width 14m, length 190m)
            CreateBlock("Main Avenue", new Vector3(0f, 0.02f, 15f), new Vector3(14f, 0.04f, 190f), asphaltMaterial);

            // Cross Streets
            CreateBlock("Plaza Cross Avenue South", new Vector3(0f, 0.03f, -14f), new Vector3(90f, 0.04f, 10f), asphaltMaterial);
            CreateBlock("Plaza Cross Avenue North", new Vector3(0f, 0.03f, 22f), new Vector3(90f, 0.04f, 10f), asphaltMaterial);
        }

        private void CreateSidewalks()
        {
            // Left & Right Sidewalks along Main Avenue (height 0.2m)
            CreateBlock("Sidewalk Left South", new Vector3(-9.5f, 0.10f, -42f), new Vector3(5f, 0.20f, 52f), sidewalkMaterial);
            CreateBlock("Sidewalk Right South", new Vector3(9.5f, 0.10f, -42f), new Vector3(5f, 0.20f, 52f), sidewalkMaterial);

            CreateBlock("Sidewalk Left North", new Vector3(-9.5f, 0.10f, 52f), new Vector3(5f, 0.20f, 56f), sidewalkMaterial);
            CreateBlock("Sidewalk Right North", new Vector3(9.5f, 0.10f, 52f), new Vector3(5f, 0.20f, 56f), sidewalkMaterial);
        }

        private void CreateMonumentPlaza()
        {
            var plazaRoot = new GameObject("District 8 Central Plaza");
            plazaRoot.transform.position = new Vector3(0f, 0f, 4f);

            // Tiered Plaza Foundation Pedestal
            var tier1 = CreateBlock("Plaza Tier 1", new Vector3(0f, 0.18f, 4f), new Vector3(32f, 0.36f, 32f), sidewalkMaterial);
            tier1.transform.SetParent(plazaRoot.transform, true);

            var tier2 = CreateBlock("Plaza Tier 2", new Vector3(0f, 0.50f, 4f), new Vector3(20f, 0.36f, 20f), buildingConcreteMaterial);
            tier2.transform.SetParent(plazaRoot.transform, true);

            // The Monolith of the Eighth Cycle
            var monolith = CreateBlock("Monolith of Cycle VIII", new Vector3(0f, 6.5f, 4f), new Vector3(4f, 12f, 4f), buildingAccentMaterial);
            monolith.transform.SetParent(plazaRoot.transform, true);

            // Monolith Spire Gold Top
            var spire = CreateBlock("Monolith Spire", new Vector3(0f, 13.5f, 4f), new Vector3(2f, 2.5f, 2f), monumentBronzeMaterial);
            spire.transform.SetParent(plazaRoot.transform, true);

            // Bronze Commemorative Plaque on South Face
            var plaque = CreateBlock("Memorial Plaque", new Vector3(0f, 1.8f, 1.9f), new Vector3(2.4f, 1.4f, 0.12f), monumentBronzeMaterial);
            plaque.transform.SetParent(plazaRoot.transform, true);

            // Four Uplights Around Monolith
            var lightOffsets = new[] { new Vector3(-5f, 0.8f, -5f), new Vector3(5f, 0.8f, -5f), new Vector3(-5f, 0.8f, 5f), new Vector3(5f, 0.8f, 5f) };
            foreach (var offset in lightOffsets)
            {
                var lightObj = new GameObject("Plaza Spot");
                lightObj.transform.position = new Vector3(0f, 0f, 4f) + offset;
                var ptLight = lightObj.AddComponent<Light>();
                ptLight.type = LightType.Point;
                ptLight.color = new Color(0.95f, 0.88f, 0.70f);
                ptLight.range = 9f;
                ptLight.intensity = 1.6f;
                lightObj.transform.SetParent(plazaRoot.transform, true);
            }
        }

        private void CreateCommercialAndResidentialBlocks()
        {
            var buildingsRoot = new GameObject("City Buildings Group");

            // South Corridor (Near Spawn)
            CreateBuildingComplex("Aura Residential Tower A", new Vector3(-20f, 0f, -48f), 18f, 16f, 26f, buildingsRoot.transform);
            CreateBuildingComplex("Aura Commercial Complex B", new Vector3(20f, 0f, -48f), 18f, 16f, 32f, buildingsRoot.transform);
            CreateBuildingComplex("District 8 Housing C", new Vector3(-20f, 0f, -28f), 18f, 16f, 22f, buildingsRoot.transform);
            CreateBuildingComplex("Metropolitan Archives D", new Vector3(20f, 0f, -28f), 18f, 16f, 28f, buildingsRoot.transform);

            // North Corridor (Towards Gate 8)
            CreateBuildingComplex("North Housing Block E", new Vector3(-20f, 0f, 36f), 16f, 16f, 24f, buildingsRoot.transform);
            CreateBuildingComplex("Council Office Block F", new Vector3(20f, 0f, 36f), 18f, 16f, 34f, buildingsRoot.transform);
            CreateBuildingComplex("Research Annex G", new Vector3(-20f, 0f, 62f), 18f, 16f, 20f, buildingsRoot.transform);
            CreateBuildingComplex("Security Sector Depot H", new Vector3(20f, 0f, 62f), 18f, 16f, 25f, buildingsRoot.transform);
        }

        private void CreateBuildingComplex(string name, Vector3 basePos, float width, float depth, float height, Transform parent)
        {
            var bld = new GameObject(name);
            bld.transform.position = basePos;
            bld.transform.SetParent(parent, false);

            // Main Concrete Body
            var body = CreateBlock("Body", new Vector3(basePos.x, height * 0.5f, basePos.z), new Vector3(width, height, depth), buildingConcreteMaterial);
            body.transform.SetParent(bld.transform, true);

            // Ground Floor Storefront Recess
            var entrance = CreateBlock("Storefront Entrance", new Vector3(basePos.x + (basePos.x < 0 ? width * 0.42f : -width * 0.42f), 1.6f, basePos.z), new Vector3(2.5f, 3.2f, 5.5f), buildingAccentMaterial);
            entrance.transform.SetParent(bld.transform, true);

            // Glowing Window Grid Stripes
            var windowFloorCount = Mathf.FloorToInt(height / 5f);
            for (var f = 1; f < windowFloorCount; f++)
            {
                var yPos = f * 5f;
                var facingOffset = basePos.x < 0 ? width * 0.51f : -width * 0.51f;
                var windowSlab = CreateBlock($"Window Row {f}", new Vector3(basePos.x + facingOffset, yPos, basePos.z), new Vector3(0.15f, 1.4f, depth * 0.75f), windowGlowMaterial);
                windowSlab.transform.SetParent(bld.transform, true);
            }
        }

        private void CreateTheAlleyway()
        {
            var alleyRoot = new GameObject("The Alleyway Sector 7");

            // Narrow alley floor leading west from Main Street (z = 32, x from -12 to -38)
            CreateBlock("Alley Floor", new Vector3(-25f, 0.04f, 32f), new Vector3(26f, 0.06f, 5.5f), alleyDarkMaterial);

            // Overhead fire escapes / pipes connecting buildings
            CreateBlock("Overhead Pipe 1", new Vector3(-22f, 4.8f, 32f), new Vector3(1.2f, 0.8f, 5.4f), buildingAccentMaterial);
            CreateBlock("Overhead Pipe 2", new Vector3(-32f, 6.2f, 32f), new Vector3(0.9f, 0.9f, 5.4f), buildingAccentMaterial);

            // Trash dumpsters & electrical generator
            CreateBlock("Industrial Generator", new Vector3(-26f, 1.1f, 34f), new Vector3(3f, 2.2f, 1.6f), buildingAccentMaterial);
            CreateBlock("Dumpster", new Vector3(-31f, 0.9f, 30f), new Vector3(2.4f, 1.8f, 1.5f), buildingConcreteMaterial);

            // Sealed Iron Doorway at the back of the alley (x = -37.5, z = 32)
            var door = CreateBlock("Sealed Maintenance Door", new Vector3(-37.5f, 1.6f, 32f), new Vector3(0.3f, 3.2f, 2.2f), barrierWarningMaterial);
            door.transform.SetParent(alleyRoot.transform, true);

            // Flickering Warning Light
            var flickLightObj = new GameObject("Flickering Alley Light");
            flickLightObj.transform.position = new Vector3(-35f, 3.8f, 32f);
            flickeringAlleyLight = flickLightObj.AddComponent<Light>();
            flickeringAlleyLight.type = LightType.Point;
            flickeringAlleyLight.color = new Color(1.0f, 0.45f, 0.15f);
            flickeringAlleyLight.range = 8.5f;
            flickeringAlleyLight.intensity = 1.4f;
            flickLightObj.transform.SetParent(alleyRoot.transform, true);
        }

        private void CreateNorthPerimeterGate()
        {
            var gateRoot = new GameObject("Perimeter Wall & Gate 8");

            // Concrete Barrier Walls (left and right)
            CreateBlock("Perimeter Wall Left", new Vector3(-24f, 5f, 86f), new Vector3(32f, 10f, 3f), buildingConcreteMaterial);
            CreateBlock("Perimeter Wall Right", new Vector3(24f, 5f, 86f), new Vector3(32f, 10f, 3f), buildingConcreteMaterial);

            // Massive Steel Gate Columns
            CreateBlock("Gate Column Left", new Vector3(-7f, 6f, 86f), new Vector3(2.8f, 12f, 4f), buildingAccentMaterial);
            CreateBlock("Gate Column Right", new Vector3(7f, 6f, 86f), new Vector3(2.8f, 12f, 4f), buildingAccentMaterial);

            // Gate Crossbeam
            CreateBlock("Gate Lintel", new Vector3(0f, 11f, 86f), new Vector3(16.8f, 2.4f, 3.6f), buildingAccentMaterial);

            // Sealed Gate 8 Blast Door
            CreateBlock("Gate 8 Blast Door", new Vector3(0f, 4.5f, 86f), new Vector3(12f, 9f, 0.8f), barrierWarningMaterial);

            // Red Warning Beacons
            var leftBeacon = new GameObject("Left Warning Beacon");
            leftBeacon.transform.position = new Vector3(-7f, 12.8f, 85f);
            var l1 = leftBeacon.AddComponent<Light>();
            l1.type = LightType.Point;
            l1.color = new Color(0.95f, 0.2f, 0.1f);
            l1.range = 7f;
            l1.intensity = 2f;
            leftBeacon.transform.SetParent(gateRoot.transform, true);

            var rightBeacon = new GameObject("Right Warning Beacon");
            rightBeacon.transform.position = new Vector3(7f, 12.8f, 85f);
            var l2 = rightBeacon.AddComponent<Light>();
            l2.type = LightType.Point;
            l2.color = new Color(0.95f, 0.2f, 0.1f);
            l2.range = 7f;
            l2.intensity = 2f;
            rightBeacon.transform.SetParent(gateRoot.transform, true);
        }

        private void CreateSkylineBackdrop()
        {
            var skylineRoot = new GameObject("Skyline Backdrop (Distant Towers)");

            var skylineData = new[]
            {
                new Vector3(-55f, 35f, 120f), new Vector3(24f, 70f, 20f),
                new Vector3(-20f, 45f, 135f), new Vector3(28f, 90f, 24f),
                new Vector3(20f, 40f, 130f), new Vector3(26f, 80f, 22f),
                new Vector3(60f, 32f, 125f), new Vector3(22f, 64f, 18f),
                new Vector3(-70f, 28f, 50f), new Vector3(20f, 56f, 20f),
                new Vector3(70f, 28f, 50f), new Vector3(20f, 56f, 20f),
            };

            for (var i = 0; i < skylineData.Length; i += 2)
            {
                var pos = skylineData[i];
                var scale = skylineData[i + 1];
                var tower = CreateBlock($"Distant Skyscraper {i / 2 + 1}", pos, scale, buildingConcreteMaterial);
                tower.transform.SetParent(skylineRoot.transform, true);
            }
        }

        private void CreateStreetFurnitureAndProps()
        {
            var propsRoot = new GameObject("Street Props and Lamps");

            // Street Lamps along Main Avenue
            var lampZPositions = new[] { -52f, -36f, -20f, 28f, 46f, 64f };
            foreach (var z in lampZPositions)
            {
                CreateStreetLamp(new Vector3(-7.5f, 0f, z), propsRoot.transform);
                CreateStreetLamp(new Vector3(7.5f, 0f, z), propsRoot.transform);
            }

            // Public Benches & Planters in Central Promenade
            CreateBench(new Vector3(-8.8f, 0.4f, -44f), propsRoot.transform);
            CreateBench(new Vector3(8.8f, 0.4f, -44f), propsRoot.transform);
            CreateBench(new Vector3(-8.8f, 0.4f, -28f), propsRoot.transform);
            CreateBench(new Vector3(8.8f, 0.4f, -28f), propsRoot.transform);

            CreatePlanterTree(new Vector3(-8.8f, 0f, -36f), propsRoot.transform);
            CreatePlanterTree(new Vector3(8.8f, 0f, -36f), propsRoot.transform);
            CreatePlanterTree(new Vector3(-8.8f, 0f, 48f), propsRoot.transform);
            CreatePlanterTree(new Vector3(8.8f, 0f, 48f), propsRoot.transform);

            // Information Kiosk / Bulletin Board near Plaza entrance
            var kiosk = CreateBlock("Citizen Information Kiosk", new Vector3(-8f, 1.4f, -10f), new Vector3(1.8f, 2.8f, 0.8f), buildingAccentMaterial);
            kiosk.transform.SetParent(propsRoot.transform, true);
        }

        private void CreateStreetLamp(Vector3 groundPos, Transform parent)
        {
            var pole = CreateBlock("Lamp Pole", groundPos + new Vector3(0f, 2.5f, 0f), new Vector3(0.24f, 5.0f, 0.24f), buildingAccentMaterial);
            pole.transform.SetParent(parent, true);

            var armOffset = groundPos.x < 0 ? 0.6f : -0.6f;
            var head = CreateBlock("Lamp Head", groundPos + new Vector3(armOffset, 5.0f, 0f), new Vector3(0.6f, 0.25f, 0.4f), lampGoldMaterial);
            head.transform.SetParent(parent, true);

            var lightObj = new GameObject("Street Lamp Light");
            lightObj.transform.position = groundPos + new Vector3(armOffset, 4.8f, 0f);
            var ptLight = lightObj.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(1.0f, 0.88f, 0.65f);
            ptLight.range = 8.5f;
            ptLight.intensity = 1.35f;
            lightObj.transform.SetParent(parent, true);
        }

        private void CreateBench(Vector3 pos, Transform parent)
        {
            var bench = CreateBlock("Bench", pos, new Vector3(1.2f, 0.45f, 2.4f), buildingAccentMaterial);
            bench.transform.SetParent(parent, true);
        }

        private void CreatePlanterTree(Vector3 pos, Transform parent)
        {
            // Concrete Planter Box
            var box = CreateBlock("Planter Box", pos + new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.9f, 1.8f), sidewalkMaterial);
            box.transform.SetParent(parent, true);

            // Stylized Tree Trunk
            var trunk = CreateBlock("Trunk", pos + new Vector3(0f, 1.8f, 0f), new Vector3(0.4f, 2.2f, 0.4f), buildingAccentMaterial);
            trunk.transform.SetParent(parent, true);

            // Stylized Foliage Cube
            var foliage = CreateBlock("Foliage", pos + new Vector3(0f, 3.4f, 0f), new Vector3(1.9f, 1.8f, 1.9f), foliageMaterial);
            foliage.transform.SetParent(parent, true);
        }

        private void CreatePlayerAndCamera()
        {
            playerObject = new GameObject("Eidon");
            playerObject.transform.position = new Vector3(0f, 1.1f, -42f);

            var controller = playerObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            playerController = playerObject.AddComponent<EidonPlayerController>();

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Eidon Visual Capsule";
            visual.transform.SetParent(playerObject.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var visualMat = CreateMaterial(new Color(0.20f, 0.50f, 0.85f));
            visual.GetComponent<Renderer>().material = visualMat;
            Destroy(visual.GetComponent<Collider>());

            var cameraObject = new GameObject("Third Person Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();

            var cameraController = cameraObject.AddComponent<ThirdPersonCameraController>();
            cameraController.SetTarget(playerObject.transform);
            playerController.SetCameraTransform(cameraObject.transform);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void RegisterInteractionPoints()
        {
            interactiveAnchors.Add(new InteractiveAnchor
            {
                Position = new Vector3(0f, 0.5f, 2f),
                Radius = 4.5f,
                Prompt = "Periksa Monumen Siklus VIII",
                Title = "THE MONOLITH OF CYCLE VIII",
                Description = "\"Diukir dalam perunggu: 'Dunia Kedelapan berdiri atas dasar keharmonisan. Jangan mencari apa yang telah runtuh, karena hidup itu sederhana.'\"\n\nEidon merasakan getaran aneh di pelipisnya. Kata-kata itu terasa seperti peringatan, bukan perayaan."
            });

            interactiveAnchors.Add(new InteractiveAnchor
            {
                Position = new Vector3(-8f, 1f, -10f),
                Radius = 3.2f,
                Prompt = "Baca Direktif Dewan Kota",
                Title = "COUNCIL DIRECTIVE 88",
                Description = "\"Seluruh warga wajib melaporkan fenomena déjà vu, memori yang tidak terdaftar, atau ilusi penglihatan kepada Administrator Sektor 8. Stabilitas pikiran adalah tanggung jawab bersama.\""
            });

            interactiveAnchors.Add(new InteractiveAnchor
            {
                Position = new Vector3(-37f, 1.5f, 32f),
                Radius = 3.5f,
                Prompt = "Periksa Pintu Besi Berkarat",
                Title = "MAINTENANCE SECTOR 07 — TERKUNCI",
                Description = "\"Pintu besi bertuliskan 'RESTRICTED SECTOR 7'. Dari balik celah pintu yang berkarat, tercium aroma ozon terbakar dan desis halus suara air mengalir di tempat yang seharusnya kering... Sesuatu di balik sini terasa salah.\""
            });

            interactiveAnchors.Add(new InteractiveAnchor
            {
                Position = new Vector3(0f, 1.5f, 84f),
                Radius = 4.2f,
                Prompt = "Periksa Gerbang Perimeter 8",
                Title = "NORTH PERIMETER BLAST GATE",
                Description = "\"Tembok raksasa setebal 3 meter memblokir jalan ke utara. Di balik gerbang ini terbentang gurun abu dari siklus-siklus sebelumnya. Lampu peringatan merah menyala tanpa henti, menegaskan bahwa tidak ada seorang pun yang diizinkan keluar.\""
            });
        }

        private static GameObject CreateBlock(string blockName, Vector3 position, Vector3 scale, Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = blockName;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().material = material;
            return block;
        }
    }
}

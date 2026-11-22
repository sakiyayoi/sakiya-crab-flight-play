"""Offline geometry and regression tests; these do not run the Unity game."""
import importlib.util
import json
from copy import deepcopy
from pathlib import Path
import unittest

SPEC = importlib.util.spec_from_file_location("density", Path(__file__).resolve().parents[1]/"author-neon-density6.py")
M = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(M)


class PlacementTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.outputs, cls.audit = M.generate()
        cls.geo = M.load_geometry()
        cls.base = M.baseline()
        cls.original = M.base_layout()["instances"]
        cls.rows = json.loads(cls.outputs[M.DATA+"Placements.json"])["instances"]
        cls.supports = json.loads(cls.outputs[M.DATA+"Supports.json"])

    def test_original_audit_reproduced(self):
        before = self.audit["before"]
        self.assertEqual((before["legacyFloatAboveGround"], before["corridorAdded"],
                          len(before["isolated"]), len(before["sideOutsideChain"]),
                          before["corridorFlightPositiveRoute"]), (229, 254, 39, 64, 385))

    def test_regeneration_matches_committed_outputs(self):
        for path, content in self.outputs.items():
            self.assertEqual((M.ROOT/path).read_text(), content, path)

    def test_planter_geometry_resolves_unknowns(self):
        rows = [r for r in self.rows if r["asset"] == "rail_glass_planter_display_r10"]
        self.assertEqual(len(rows), 28)
        self.assertEqual(self.geo[rows[0]["asset"]]["bounds"][1][0], .02)
        self.assertTrue(all(abs(M.bottom(r, self.geo)) < M.EPSILON for r in rows))

    def test_final_support_chain_and_density(self):
        a = self.audit["after"]
        self.assertEqual(a["unsupported"], 0)
        self.assertEqual(a["isolated"], [])
        self.assertEqual(a["sideOutsideChain"], [])
        self.assertLessEqual(a["rolling100mPeakAfterMall"], 63.55)
        self.assertLess(a["maxCorridorGap"]["metres"], self.audit["before"]["maxCorridorGap"]["metres"])
        self.assertEqual(set(map(tuple, self.audit["overlapPairsAfter"]))-
                         set(map(tuple, self.audit["overlapPairsBefore"])), set())

    def test_satellites_and_mall_preserved(self):
        old = [r for r in self.original if M.natural_air(r)]
        new = [r for r in self.rows if M.natural_air(r)]
        self.assertEqual(len(old), 5)
        self.assertEqual(old, new)
        self.assertFalse(any(M.sector(r) == "mall" for r in self.rows))
        self.assertEqual(len(self.base), 348)

    def test_floating_negative_control(self):
        rows = deepcopy(self.rows)
        rows[0]["p"][1] += .02  # 2 cm must fail, well below the old 0.5 m audit cut-off.
        issues = M.support_issues(rows, self.geo, self.supports)
        self.assertEqual([r["id"] for r in issues], [rows[0]["id"]])
        after = M.metrics(rows, self.base, self.geo, self.supports)
        with self.assertRaisesRegex(ValueError, "Unsupported"):
            M.validate(self.original, rows, self.base, self.geo, self.supports, after)

    def test_support_must_be_grounded_and_cover_footprint(self):
        bad = deepcopy(self.supports)
        bad["supports"][0]["p"][1] += .2
        with self.assertRaisesRegex(ValueError, "Ungrounded"):
            M.support_issues(self.rows, self.geo, bad)
        bad = deepcopy(self.supports)
        bad["supports"][0]["size"][0] = .1
        with self.assertRaisesRegex(ValueError, "footprint"):
            M.support_issues(self.rows, self.geo, bad)

    def test_missing_geometry_fails_closed(self):
        row = deepcopy(self.rows[0]);row["asset"] = "unknown_geometry"
        with self.assertRaisesRegex(ValueError, "Missing geometry"):
            M.snap(row, self.geo)

    def test_new_airborne_exception_rejected(self):
        rows = deepcopy(self.rows)
        row = next(r for r in rows if r["asset"] == "satellite_neon6" and r["p"][1] < 0)
        row["p"][1] = 6
        after = M.metrics(rows, self.base, self.geo, self.supports)
        with self.assertRaisesRegex(ValueError, "New airborne exception"):
            M.validate(self.original, rows, self.base, self.geo, self.supports, after)


if __name__ == "__main__":
    unittest.main(verbosity=2)

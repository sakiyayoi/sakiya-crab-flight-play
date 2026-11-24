"""Meaningful guard/negative-control checks for the new layout constraints."""
import copy
import json
import unittest
import author as A
import audit as Q

class OpeningTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data=A.build()
    def test_generated_layout_and_complete_audit(self):
        self.assertEqual(A.OUT.read_text(),A.encoded(self.data))
        self.assertEqual(Q.audit(self.data)['floating'],0)
    def test_two_centimetres_of_float_rejected(self):
        d=copy.deepcopy(self.data);d['instances'][0]['p'][1]+=.02
        with self.assertRaisesRegex(AssertionError,'floating'):Q.audit(d,False)
    def test_isolated_side_target_rejected(self):
        d=copy.deepcopy(self.data);d['instances'][0]['p'][2]=40
        with self.assertRaisesRegex(AssertionError,'isolated'):Q.audit(d,False)
    def test_detached_landmark_face_rejected(self):
        sign=copy.deepcopy(self.data['landmarks'][0]);sign['boxes'][2]['p'][2]-=1
        self.assertIn('lamp-face',Q.scenery_audit(sign)['disconnectedParts'])
    def test_hero_post_is_grounded_and_upper_is_wider(self):
        for r in self.data['instances']:
            if r.get('hero'):
                h=Q.hero_contract(r,A.geometry())
                self.assertEqual(h['base']['posts'],1)
                self.assertEqual(len(h['upper']['shardGroups']),6)
    def test_density_counts_every_registered_system(self):
        rows=self.data['instances']+A.fixed_targets(True)
        self.assertEqual([Q.metrics(rows,a,b)['count'] for _,a,b in A.SECTORS],[3,36,77])
    def test_protected_sources_and_only_coordinate_hooks(self):
        self.assertTrue(Q.source_contract()['otherPhysicsByteExact'])

if __name__=='__main__':unittest.main(verbosity=2)

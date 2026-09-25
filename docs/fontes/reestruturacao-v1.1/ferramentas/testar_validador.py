#!/usr/bin/env python3
"""Testa o verificador em cópias temporárias, nunca no repositório da aplicação."""
from __future__ import annotations

import hashlib
import json
from pathlib import Path
import shutil
import sys
import tempfile
import unittest

sys.dont_write_bytecode = True
from validar_pacote import MANIFEST, validate  # noqa: E402

SOURCE = Path(__file__).resolve().parents[1]


class ValidatorTests(unittest.TestCase):
    def setUp(self) -> None:
        self.temp = tempfile.TemporaryDirectory(prefix="wissian-package-test-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "package"
        shutil.copytree(SOURCE, self.root, ignore=shutil.ignore_patterns("__pycache__", "*.pyc"))

    def write_json(self, path: str, obj: object) -> None:
        (self.root/path).write_text(json.dumps(obj, ensure_ascii=False, indent=2)+"\n", encoding="utf-8")

    def read_json(self, path: str) -> dict:
        return json.loads((self.root/path).read_text(encoding="utf-8"))

    def rehash_fixture(self) -> None:
        """Atualiza só o manifesto da cópia para isolar testes semânticos."""
        manifest = self.read_json(MANIFEST)
        manifest["files"] = []
        for path in sorted(self.root.rglob("*")):
            if not path.is_file() or path.name == MANIFEST:
                continue
            data = path.read_bytes()
            manifest["files"].append({"path": path.relative_to(self.root).as_posix(),
                                      "bytes": len(data), "sha256": hashlib.sha256(data).hexdigest()})
        self.write_json(MANIFEST, manifest)

    def assert_error(self, prefix: str) -> None:
        result = validate(self.root)
        self.assertFalse(result["ok"], result)
        self.assertTrue(any(x.startswith(prefix) for x in result["errors"]), result["errors"])

    def test_01_clean_package(self) -> None:
        result = validate(self.root)
        self.assertTrue(result["ok"], result["errors"])

    def test_02_missing_file(self) -> None:
        (self.root/"00_COMECE_AQUI_CODEX.md").unlink()
        self.assert_error("FILE_MISSING")

    def test_03_tampered_bytes(self) -> None:
        path = self.root/"02_DECISOES_E_LIMITES.md"
        path.write_text(path.read_text(encoding="utf-8")+"alteracao\n",encoding="utf-8")
        self.assert_error("HASH_MISMATCH")

    def test_04_extra_file(self) -> None:
        (self.root/"arquivo_extra.txt").write_text("fixture\n", encoding="utf-8")
        self.assert_error("UNLISTED_FILE")

    def test_05_invalid_json(self) -> None:
        (self.root/"matrizes/requisitos.json").write_text("{invalid", encoding="utf-8")
        self.rehash_fixture()
        self.assert_error("JSON_INVALID")

    def test_06_broken_link(self) -> None:
        path=self.root/"01_LEIA_PRIMEIRO.md"
        path.write_text(path.read_text(encoding="utf-8")+"\n[teste](inexistente.md)\n", encoding="utf-8")
        self.rehash_fixture()
        self.assert_error("LINK_MISSING")

    def test_07_unknown_requirement(self) -> None:
        obj=self.read_json("matrizes/casos_de_teste.json")
        obj["cases"][0]["requirements"].append("REQ999")
        self.write_json("matrizes/casos_de_teste.json",obj)
        self.rehash_fixture()
        self.assert_error("UNKNOWN_REQUIREMENT")

    def test_08_duplicate_test_id(self) -> None:
        obj=self.read_json("matrizes/casos_de_teste.json")
        obj["cases"][1]["id"]=obj["cases"][0]["id"]
        self.write_json("matrizes/casos_de_teste.json",obj)
        self.rehash_fixture()
        self.assert_error("ID_DUPLICATE")

    def test_09_uncovered_requirement(self) -> None:
        obj=self.read_json("matrizes/requisitos.json")
        row=dict(obj["requirements"][0]); row["id"]="REQ999"
        obj["requirements"].append(row)
        self.write_json("matrizes/requisitos.json",obj)
        self.rehash_fixture()
        self.assert_error("UNCOVERED_REQUIREMENT")

    def test_10_manifest_path_traversal(self) -> None:
        obj=self.read_json(MANIFEST)
        obj["files"][0]["path"]="../outside.txt"
        self.write_json(MANIFEST,obj)
        self.assert_error("MANIFEST_PATH_UNSAFE")

    def test_11_manifest_self_reference(self) -> None:
        obj=self.read_json(MANIFEST)
        obj["files"].append({"path":MANIFEST,"bytes":0,"sha256":"0"*64})
        self.write_json(MANIFEST,obj)
        self.assert_error("MANIFEST_DUPLICATE_OR_SELF")

    def test_12_false_application_test_claim(self) -> None:
        obj=self.read_json("matrizes/casos_de_teste.json")
        obj["cases"][0]["status"]="PASS"
        self.write_json("matrizes/casos_de_teste.json",obj)
        self.rehash_fixture()
        self.assert_error("UNEXPECTED_EXECUTION_CLAIM")

    def test_13_missing_product_dependency(self) -> None:
        name="contratos/fundacao_neutra.exemplo.json"
        obj=self.read_json(name); obj["module_ids"].remove("core.audit")
        self.write_json(name,obj); self.rehash_fixture()
        self.assert_error("PRODUCT_DEPENDENCY_MISSING")

    def test_14_module_cycle(self) -> None:
        name="contratos/catalogo_modulos.exemplo.json"
        obj=self.read_json(name); obj["modules"][0]["requires"]=["core.identity"]
        self.write_json(name,obj); self.rehash_fixture()
        self.assert_error("MODULE_CYCLE")

    def test_15_false_permission_grant(self) -> None:
        name="contratos/socioeducativo.exemplo.json"
        obj=self.read_json(name); obj["authorization_grants"]=["admin"]
        self.write_json(name,obj); self.rehash_fixture()
        self.assert_error("PRODUCT_SAFETY_INVALID")

    def test_16_domain_inside_foundation(self) -> None:
        name="contratos/fundacao_neutra.exemplo.json"
        obj=self.read_json(name); obj["module_ids"] += ["cap.scheduling","soc.operations"]
        self.write_json(name,obj); self.rehash_fixture()
        self.assert_error("FOUNDATION_HAS_DOMAIN")

    def test_17_unknown_evidence(self) -> None:
        name="matrizes/inventario_inicial.json"
        obj=self.read_json(name); obj["items"][0]["evidence_id"]="E999"
        self.write_json(name,obj); self.rehash_fixture()
        self.assert_error("UNKNOWN_EVIDENCE")

    def test_18_nonexistent_root(self) -> None:
        result=validate(self.root/"not-a-folder")
        self.assertFalse(result["ok"])
        self.assertIn("ROOT_MISSING", result["errors"])

    def test_19_link_outside_package(self) -> None:
        path=self.root/"01_LEIA_PRIMEIRO.md"
        path.write_text(path.read_text(encoding="utf-8")+"\n[fora](../outside.md)\n",encoding="utf-8")
        self.rehash_fixture()
        self.assert_error("LINK_OUTSIDE_PACKAGE")

    def test_20_symlink_rejected(self) -> None:
        try:
            (self.root/"link.txt").symlink_to(self.root/"01_LEIA_PRIMEIRO.md")
        except (OSError, NotImplementedError):
            self.skipTest("Este ambiente não permite criar symlink na fixture.")
        self.assert_error("SYMLINK_FORBIDDEN")


if __name__ == "__main__":
    unittest.main(verbosity=2)

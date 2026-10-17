import json, hashlib, pathlib, zipfile, os, re, sys
root=pathlib.Path('.')
ev=root/'evidence'
zip_path=ev/'pmcro-marketplace-init.zip'
manifest=json.loads((ev/'candidate-manifest.json').read_text())
expected_zip=(ev/'zip-sha256.txt').read_text().strip()
assert hashlib.sha256(zip_path.read_bytes()).hexdigest()==expected_zip, 'zip digest mismatch'
out=root/'candidate'
out.mkdir()
with zipfile.ZipFile(zip_path) as z: z.extractall(out)
actual=[]
for rec in manifest['files']:
    p=out/rec['path']
    assert p.is_file(), f"missing {rec['path']}"
    h=hashlib.sha256(p.read_bytes()).hexdigest()
    assert h==rec['sha256'], f"hash mismatch {rec['path']}"
    actual.append(rec['path'])
all_files=sorted([str(p.relative_to(out)).replace('\\','/') for p in out.rglob('*') if p.is_file()],key=str.lower)
assert all_files==[r['path'] for r in manifest['files']], 'file set/order mismatch'
canon=''.join(f"{r['path']}\\0{r['sha256']}\\n" for r in manifest['files']).encode()
# Candidate producer used actual NUL/newline separators, not slash escapes.
canon=b''.join(r['path'].encode()+b'\x00'+r['sha256'].encode()+b'\n' for r in manifest['files'])
assert hashlib.sha256(canon).hexdigest()==manifest['candidateDigest'], 'candidate digest mismatch'
assert manifest['candidateDigest']=='54877ec85b39cb327f1e03616661b91545bba6d012f6cdfa229f924e3a39dbc5'

proj=json.loads((out/'.agents/project.json').read_text(encoding='utf-8-sig'))
assert proj['metadata']['id']=='pmcro-marketplace'
assert proj['metadata']['name']=='PMCRO Marketplace'
assert proj['authority']=={'register':False,'grant':False,'invoke':False}
app=json.loads((out/'.agents/application.json').read_text(encoding='utf-8-sig'))
assert app['apiVersion']=='agents.application/v1' and app['kind']=='Application'
assert app['spec']['governance']['checkerRequired'] is True
assert app['spec']['governance']['auditRequired'] is True
mp=json.loads((out/'.agents/marketplaces.json').read_text(encoding='utf-8-sig'))
assert mp['sources']==[], 'project must inherit global marketplace sources; no local authority/source copy'
state=json.loads((out/'.pmcro/state.json').read_text())
assert state['projectId']=='pmcro-marketplace' and state['initialized'] is True
assert state['trainingEligible'] is False
assert state['activeTrailId']=='pmcro-marketplace-trail-0001'
assert state['activeCycleId']=='cycle-0001'
seed=(out/'.pmcro/seeds/seed-0001.txt').read_text()
assert '@orchestrator /pmcro:init' in seed and 'PMCRO-Marketplace' in seed

graph=json.loads((out/'.pmcro/evidence/cycle-1/capability-graph.json').read_text())
s=graph['summary']
assert s['marketplaces']==17 and s['plugins']==17 and s['skills']==91
assert s['skillAgentFiles']==91 and s['skillScriptFiles']==21
assert s['registeredRecords']==5 and s['grantRecords']==2
assert len(graph['registeredCapabilities'])==5 and len(graph['grantRecords'])==2
assert graph['laws']==['DISCOVERY != REGISTRATION','REGISTRATION != GRANT','GRANT != ACTIVATION','ACTIVATION != EXECUTION']

prev=None
for n,role in [(1,'orchestrator'),(2,'planner'),(3,'maker')]:
    f=json.loads((out/'.pmcro/frames'/f'{n:04d}-{role}.json').read_text())
    h=f.pop('hash')
    canon=json.dumps(f,sort_keys=True,separators=(',',':'),ensure_ascii=False).encode()
    assert hashlib.sha256(canon).hexdigest()==h, f'frame hash mismatch {n}'
    if prev is not None: assert f['prev_hash']==prev, f'frame chain mismatch {n}'
    prev=h

for forbidden in ['.pmcro/grants','.pmcro/registry','.pmcro/activation','.pmcro/activations']:
    assert not (out/forbidden).exists(), f'forbidden project authority state {forbidden}'
secret_patterns=[rb'ghp_[A-Za-z0-9]{20,}',rb'github_pat_[A-Za-z0-9_]{20,}',rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----']
for p in out.rglob('*'):
    if not p.is_file(): continue
    b=p.read_bytes()
    for pat in secret_patterns:
        assert re.search(pat,b) is None, f'secret-like material in {p}'

verdict={
 'apiVersion':'pmcro.external-checker-verdict/v1','checker':'github-actions/ubuntu-latest','independent':True,
 'candidateDigest':manifest['candidateDigest'],'packageFileCount':len(manifest['files']),
 'projectContract':'PASS','authorityEmpty':'PASS','trailIntegrity':'PASS','capabilityGraph':'PASS','secretNondisclosure':'PASS',
 'verdict':'PASS','githubRunId':os.environ.get('GITHUB_RUN_ID'),'githubRunAttempt':os.environ.get('GITHUB_RUN_ATTEMPT'),'githubHeadSha':os.environ.get('GITHUB_SHA')
}
(ev/'external-checker-verdict.json').write_text(json.dumps(verdict,indent=2)+'\n')
print(json.dumps(verdict,indent=2))

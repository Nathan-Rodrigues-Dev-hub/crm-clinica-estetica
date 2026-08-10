import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { pacientesApi } from '../api/services'
import { mensagemErro } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { useToast } from '../components/Toast'
import { Badge, Card, PageHeader } from '../components/ui'
import { Spinner } from '../components/Spinner'
import { MidiaGaleria } from '../components/MidiaGaleria'
import { DetalheAnamnese } from '../components/DetalheAnamnese'
import { AnamneseManualForm } from '../components/AnamneseManualForm'
import { PacienteFormModal } from './PacienteFormModal'
import { formatarCpf, formatarData, formatarTelefone } from '../lib/format'
import type { PacienteDto } from '../api/types'

export function PacienteDetalhePage() {
  const { id = '' } = useParams()
  const navigate = useNavigate()
  const toast = useToast()
  const { temPapel } = useAuth()
  const podeVerMidia = temPapel('Admin', 'Profissional', 'SuperAdmin')
  const podeVerAnamnese = temPapel('Admin', 'Profissional', 'SuperAdmin')

  const [paciente, setPaciente] = useState<PacienteDto | null>(null)
  const [carregando, setCarregando] = useState(true)
  const [editar, setEditar] = useState(false)
  const [verAnamneses, setVerAnamneses] = useState(false)
  const [anamneseManual, setAnamneseManual] = useState(false)

  const carregar = () => {
    setCarregando(true)
    pacientesApi.obter(id).then((r) => setPaciente(r.data)).catch((e) => toast.erro(mensagemErro(e))).finally(() => setCarregando(false))
  }

  useEffect(carregar, [id])

  if (carregando) return <Spinner full label="Carregando paciente…" />
  if (!paciente) return null

  return (
    <>
      <PageHeader
        titulo={paciente.nome}
        subtitulo={paciente.origem === 'AutoCadastroLink' ? 'Cadastro via link de anamnese' : 'Cadastro pela recepção'}
        acao={
          <div className="row">
            <button className="btn btn-ghost" onClick={() => navigate('/pacientes')}>← Voltar</button>
            {podeVerAnamnese && <button className="btn btn-soft" onClick={() => setVerAnamneses(true)}>Ver anamneses</button>}
            <button className="btn btn-soft" onClick={() => setAnamneseManual(true)}>+ Anamnese manual</button>
            <button className="btn btn-primary" onClick={() => setEditar(true)}>Editar</button>
          </div>
        }
      />

      <Card>
        <div className="spread" style={{ marginBottom: 12 }}>
          <h3>Dados cadastrais</h3>
          {paciente.cpfPendente && <Badge texto="CPF pendente" tom="alerta" />}
        </div>
        <dl className="kv">
          <dt>CPF</dt><dd>{formatarCpf(paciente.cpf)}</dd>
          <dt>Telefone</dt><dd>{formatarTelefone(paciente.telefone)}</dd>
          <dt>Nascimento</dt><dd>{formatarData(paciente.dataNascimento)}</dd>
          <dt>Sexo</dt><dd>{paciente.sexo ?? '—'}</dd>
          <dt>Endereço</dt><dd>{paciente.endereco ?? '—'}</dd>
          <dt>Rede social</dt><dd>{paciente.redeSocial ?? '—'}</dd>
          <dt>Cadastrado em</dt><dd>{formatarData(paciente.criadoEm)}</dd>
        </dl>
      </Card>

      <div style={{ marginTop: 18 }}>
        {podeVerMidia ? (
          <MidiaGaleria pacienteId={paciente.id} />
        ) : (
          <Card>
            <div className="alert alert-info" style={{ margin: 0 }}>
              A galeria de mídia (fotos clínicas) é restrita a Admin e Profissional.
            </div>
          </Card>
        )}
      </div>

      <PacienteFormModal aberto={editar} paciente={paciente} onFechar={() => setEditar(false)} onSalvo={(p) => setPaciente(p)} />
      {podeVerAnamnese && (
        <DetalheAnamnese pacienteId={paciente.id} aberto={verAnamneses} onFechar={() => setVerAnamneses(false)} />
      )}
      <AnamneseManualForm
        pacienteId={paciente.id}
        aberto={anamneseManual}
        onFechar={() => setAnamneseManual(false)}
        onCriada={() => setAnamneseManual(false)}
      />
    </>
  )
}

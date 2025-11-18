using FerramentariaTest.Entities;
using FerramentariaTest.EntitiesBS;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace FerramentariaTest.Models
{
    public class AuditLogModel
    {
        public string? Message { get; set; }
        public string? TimeStamp { get; set; }
        public string? Outcome { get; set; }
        public PropertyModel? Property { get; set; }
        public string? TransactionId { get; set; }
    }


    public class PropertyModel
    {
        public Properties? Properties { get; set; }
    
    }

    public class Properties
    {
        [JsonProperty("FinalAuditResultModel")]
        public FinalAuditResultModel? FinalAuditResultModel { get; set; }
        [JsonProperty("ActionId")]
        public string? ActionId { get; set; }
        [JsonProperty("ActionName")]
        public string? ActionName { get; set; }
        [JsonProperty("RequestId")]
        public string? RequestId { get; set; }
        [JsonProperty("RequestPath")]
        public string? RequestPath { get; set; }
        [JsonProperty("MachineName")]
        public string? MachineName { get; set; }
        [JsonProperty("SpanId")]
        public string? SpanId { get; set; }
        [JsonProperty("ParentId")]
        public string? ParentId { get; set; }
        [JsonProperty("Application")]
        public string? Application { get; set; }
        [JsonProperty("Environment")]
        public string? Environment { get; set; }
        [JsonProperty("LogType")]
        public string? LogType { get; set; }
        //[JsonProperty("ClientIp")]
        //public string? ClientIp { get; set; }
    }

    public class combineFixModel
    {
        public string? TransactionId { get; set; }
        public string? CrachaNumber { get; set; }
        public UserClaimModel Balconista { get; set; } = new UserClaimModel();
        public fnRetornaColaboradorCracha CrachaInformation { get; set; } = new fnRetornaColaboradorCracha();
        public EmployeeInformationBS Employee { get; set; } = new EmployeeInformationBS();

    }

    public class FinalAuditResultModel
    {
        public string? TransactionId { get; set; }
        public AuditBalconistaModel? Balconista { get; set; }
        public AuditSolicitanteModel? Requester { get; set; }
        public List<AuditMaterialsModel> Materials { get; set; } = new List<AuditMaterialsModel>();
    }

    public class AuditBalconistaModel
    {
        public string? BalconistaChapa { get; set; }
        public string? BalconistaNome { get; set; }
    }

    public class AuditSolicitanteModel
    {
        public string? CrachaTypeRequester { get; set; }
        public string? CrachaNoRequester { get; set; }
        public string? ChapaRequester { get; set; }
        public string? NameRequester { get; set; }
        public string? FunctionRequester { get; set; }
        public string? SectionRequester { get; set; }
    }


    public class AuditMaterialsModel
    {
        public string? Code { get; set; }
        public string? Catalog { get; set; }
        //public string? CatalogClass { get; set; }
        //public string? CatalogType { get; set; }
        public string? CatalogDescription { get; set; }
        public int? QtyRequested { get; set; }
        public string? ChapaLiberador { get; set; }
        public string? NomeLiberador { get; set; }
        public string? Obra { get; set; }
        public string? ObservacaoBalconista { get; set; }
    }


    public class TermsControlResultModel
    {
        public int? Id { get; set; }
        public int? Balconista { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? Responsavel { get; set; }
        public string? DataRegistroString { get; set; }
        public int? CodPessoa { get; set; }
        public byte[]? ImageData { get; set; }
    }

    public class TermsControlModel
    {
        public int? Id { get; set; }
        public int? Balconista { get; set; }
        public string? Chapa { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? DataRegistro { get; set; }
        public byte[]? ImageData { get; set; }
    }

    public class RetiradaOrderPageModel
    {
        public string? TransactionId { get; set; }
        public List<newCatalogInformationModel> RetiradaOrder { get; set; } = new List<newCatalogInformationModel>();
    }

    public class CompleteReservationPageModel
    {
        public string? TransactionId { get; set; }
        public List<FinalReservationResult> FinalOrder { get; set; } = new List<FinalReservationResult>();
    }

    public class ProcessListReservationModel
    {
        public string? TransactionId { get; set; }
        public List<ReservedProductModel> ReservedItems { get; set; } = new List<ReservedProductModel>();
    }


    public class AddLeaderSubmitModel
    {
        public int? CodPessoaMember { get; set; }
        public int? IdTerceiroMember { get; set; }
        public string? ChapaMember { get; set; }
        public string? NomeMember { get; set; }
        public int? Ativo { get; set; }
        public DateTime? DataRegistro { get; set; }
        public int? IdUser { get; set; }
        
    }

    public class FuncionarioModel
    {
        public int? IdTerceiro { get; set; }
        public int? CodPessoa { get; set; }
        public short? CodColigada { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? CodRecebimento { get; set; }
        public string? CodSituacao { get; set; }
        public string? CodTipo { get; set; }
        public string? CodSecao { get; set; }
        public string? Secao { get; set; }
        public string? CodFuncao { get; set; }
        public string? Funcao { get; set; }
        public DateTime? DataMudanca { get; set; }
        public string? Sexo { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public DateTime? DataDemissao { get; set; }
        public DateTime? FimProgFerias1 { get; set; }
        public DateTime? DataRegistro { get; set; }
        public byte[]? IMAGEM { get; set; }
        public string? ImageStringByte { get; set; }
        public bool? IsRegistered { get; set; }
        public int? IdUserSib { get; set; }
        public int? IdLeader { get; set; }
        public int? AtivoLeader { get; set; }

    }
    public class ReservationsControlModel
    {
        public int? ControlId { get; set; } 
        public string? Chave { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ControlStatus { get; set; }
        public int? LeadercodPessoa { get; set; }
        public string? LeaderName { get; set; }
        public string? ControlStatusString { get; set; }
        public int? reserveStatus { get; set; }
        public string? reserveStatusString { get; set; }
        public string? controlDataRegistroString { get; set; }
    }
    public class ReservationsModel
    {
        public int? ControlId { get; set; }
        public int? itemCount { get; set; }
        public string? Chave { get; set; }
        public string? LeaderName { get; set; }
        public int? RegisteredCount { get; set; }
        public int? PreparingCount { get; set; }
        public int? PreparedCount { get; set; }
        public string? ActualStatus { get; set; }
        public int? GroupStatus { get; set; }
        public string? controlDataRegistroString { get; set; }
    }

    public class PrepareItemsPageModel
    {
        public int? ControlId { get; set; }
        public string? RegisteredDate { get; set; }
        public string? ExpiryDateString { get; set; }
        public List<ItemReservationDetailModel> items { get; set; } = new List<ItemReservationDetailModel>();
    }

    public class ItemReservationDetailModel
    {
        public int? IdFerramentaria { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdCatalogo { get; set; }
        public int? IdReservation { get; set; }
        public int? intClasse { get; set; }
        public string? Classe { get; set; }
        public string? Type { get; set; }
        public string? Codigo { get; set; }
        public string? itemNome { get; set; }
        public int? QuantidadeResquested { get; set; }
        public string? DataRegistro { get; set; }
        public string? Status { get; set; }
        public int? intStatus { get; set; }
        public bool IsSelected { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? ExpiryDateString { get; set; }
        public int? ferramentariacount { get; set; }
        public int? MemberCodPessoa { get; set; }
        public int? LeaderCodPessoa { get; set; }
        public int? IdObra { get; set; }
        public string? ObraName { get; set; }
        public employeeNewInformationModel? MemberInfo { get; set; } = new employeeNewInformationModel();
        public employeeNewInformationModel? LeaderInfo { get; set; } = new employeeNewInformationModel();
        public List<ControleCA>? listCA { get; set; } = new List<ControleCA>();
        public List<Ferramentaria>? listFerramentaria { get; set; } = new List<Ferramentaria>();
        public List<productDetail>? listProduto { get; set; } = new List<productDetail>();

        public int? IdFerramentariaFrom { get; set; }
        public string? FerramentariaFrom { get; set; }
        public int? IdFerramentariaTo { get; set; }
        public string? FerramentariaTo { get; set; }

    }

    public class employeeNewInformationModel
    {
        public int? Id { get; set; }
        public int? CodPessoa { get; set; }
        public int? IdTerceiro { get; set; }
        public byte[]? Image { get; set; }
        public string? Imagebase64 { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? CodSituacao { get; set; }
        public DateTime? DataDemissao { get; set; }
        public DateTime? DataAdmissao { get; set; }
        public short? CodColigada { get; set; }
        public string? Funcao { get; set; }
        public string? Secao { get; set; }

        public List<MensagemSolicitanteViewModel>? blockMessage { get; set; } = new List<MensagemSolicitanteViewModel>();
        public string? blockSolicitanteMessage { get; set; }
        public string? CellphoneNo { get; set; }

    }

    public class productDetail
    {
        public int? Id { get; set; }
        public int? IdCatalogo { get; set; }
        public int? Quantidade { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public DateTime? DataVencimento { get; set; }
        public List<ControleCA>? listCA { get; set; } = new List<ControleCA>();
        public DateTime? DataPrevistaDevolucao { get; set; }
        public bool allowedtoborrow { get; set; }
        public string? reason { get; set; }
    }


    public class reserveSubmission
    {
        public int? IdProduto { get; set; }
        public int? IdReservation { get; set; }
        public int? IdControleCA { get; set; }
        public DateTime? dataPrevista { get; set; }
        public string? observacao { get; set; }
    }

    public class cancellationSubmission
    {
        public int? IdReservation { get; set; }
        public string? obsCancel { get; set; }
    }

    public class transferSubmission
    {

        public int? IdReservation { get; set; }
        public int? IdFerramentariaOrigin { get; set; }
        public int? IdFerramentariaDestination { get; set; }
        public string? obsTransfer { get; set; }

    }


    public class groupedReservedItems
    {
        public int? IdReserveControl { get; set; }
        public string? Cracha { get; set; }
        public employeeNewInformationModel? MemberInfo { get; set; } = new employeeNewInformationModel();
        public employeeNewInformationModel? LeaderInfo { get; set; } = new employeeNewInformationModel();
        public List<ProductReservationModel>? reservedItems { get; set; } = new List<ProductReservationModel>();
    }

    public class ProductReservationModel
    {
        public int? IdProductReservation { get; set; }
        public int? IdControleCA { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public string? Observacao { get; set; }
        public int? PreparedBy { get; set; }


        public int? IdProduto { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }

        public int? intClasse { get; set; }
        public string? Classe { get; set; }

        public string? Type { get; set; }
        public string? Codigo { get; set; }
        public string? itemNome { get; set; }

        public int? QuantidadeResquested { get; set; }
        public int? MemberCodPessoa { get; set; }
        public employeeNewInformationModel? MemberInfo { get; set; } = new employeeNewInformationModel();
        public int? IdObra { get; set; }
        public string? ObraName { get; set; }

        public int? IdReservationControl { get; set; }
        public int? LeaderCodPessoa { get; set; }
        public employeeNewInformationModel? LeaderInfo { get; set; } = new employeeNewInformationModel();

        public balconistaInformation? PreparedByInfo { get; set; } = new balconistaInformation();


    }

    public class balconistaInformation
    {
        public int? Id { get; set; }
        public string? Chapa { get; set; }
        public string? Nome { get; set; }
        public string? Departamento { get; set; }
        public string? Secao { get; set; }
        public string? Funcao { get; set; }
    }

    public class newCatalogInformationModel
    {
        
        public int? IdCatalogo { get; set; }
        public int? IdCategoria { get; set; }
        public int? intClasse { get; set; }
        public string? Classe { get; set; }
        public string? Type { get; set; }
        public string? Codigo { get; set; }
        public string? itemNome { get; set; }
        public int? DataDeRetornoAutomatico { get; set; }
        public DateTime? DataReturn { get; set; }

        public int? IdReservationControl { get; set; }
        public int? IdReservation { get; set; }
        public int? QuantidadeResquested { get; set; }
        public int? IdObra { get; set; }
        public string? ObraName { get; set; }

        public int? IdCASelected { get; set; }
        public List<ControleCA>? listCA { get; set; } = new List<ControleCA>();

        public int? IdProdutoSelected { get; set; }
        public List<newProductInformation>? listProducts { get; set; } = new List<newProductInformation>();

        public int? MemberCodPessoa { get; set; }
        public employeeNewInformationModel? MemberInfo { get; set; } = new employeeNewInformationModel();

        public int? LeaderCodPessoa { get; set; }
        public employeeNewInformationModel? LeaderInfo { get; set; } = new employeeNewInformationModel();

        public bool? IsTransferable { get; set; }

    }

    public class newProductInformation
    {
        public int? IdProduto { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? AF { get; set; }
        public int? PAT { get; set; }
        public int? StockQuantity { get; set; }
        public DateTime? DataVencimento { get; set; }
        public bool AllowedToBorrow { get; set; }
        public string? Reason { get; set; }
    }

    public class ReservedProductModel
    {
        public int? IdReservationControl { get; set; }
        public int? IdReservation { get; set; }
        public int? IdCatalogo { get; set; }
        public int? IdFerramentaria { get; set; }
        public int? IdProduto { get; set; }
        public int? intClasse { get; set; }

        public string? Classe { get; set; }
        public string? Type { get; set; }
        public string? Codigo { get; set; }
        public string? itemNome { get; set; }
        public string? Status { get; set; }
        public int? intStatus { get; set; }

        public int? MemberCodPessoa { get; set; }
        public int? LeaderCodPessoa { get; set; }
        public int? IdObra { get; set; }
        public string? ObraName { get; set; }

        public employeeNewInformationModel? MemberInfo { get; set; } = new employeeNewInformationModel();
        public employeeNewInformationModel? LeaderInfo { get; set; } = new employeeNewInformationModel();
        public List<ControleCA>? listCA { get; set; } = new List<ControleCA>();


        public int? QtyRequested { get; set; }
        public int? QtyStock { get; set; }

        public bool? IsTransferable { get; set; }
    }

    public class FinalSubmissionProcess
    {
        public bool IsSelected { get; set; }
        public int? IdProductReservation { get; set; }
        public int? IdReservationControl { get; set; }
        public int? IdReservation { get; set; }
        public int? IdControleCA { get; set; }
        public int? IdProduto { get; set; }
        public int? QtyRequested { get; set; }

        public string? PorType { get; set; }
        public int? Classe { get; set; }

        public int? IdTerceiroSolicitante { get; set; }
        public string? ChapaSolicitante { get; set; }
        public short? CodColigadaSolicitante { get; set; }

        public int? IdTerceiroLiberador { get; set; }
        public string? ChapaLiberador { get; set; }
        public short? CodColigadaLiberador { get; set; }

        public int? IdObra { get; set; }

        public string? Observacao { get; set; }

        public DateTime? DateReturn { get; set; }
        public string? CrachaNo { get; set; }
    }

    public class CommonDataProduct
    {
        public int? Classe { get; set; }
        public string? CatalogoType { get; set; }
        public int? IdReservation { get; set; }
        public int? IdProduto { get; set; }
        public int? Solicitante_IdTerceiro { get; set; }
        public int? Solicitante_CodColigada { get; set; }
        public string? Solicitante_Chapa { get; set; }
        public int? Liberador_IdTerceiro { get; set; }
        public int? Liberador_CodColigada { get; set; }
        public string? Liberador_Chapa { get; set; }
        public string? Observacao { get; set; }
        public DateTime? DataEmprestimo { get; set; }
        public DateTime? DataPrevistaDevolucao { get; set; }
        public int? IdObra { get; set; }
        public int? Quantidade { get; set; }
        public int? IdFerrOndeProdRetirado { get; set; }
        public int? IdControleCA { get; set; }

    }

    public class FinalReservationResult
    {
        public int? IdProductReservation { get; set; }
        public int? IdReservationControl { get; set; }
        public int? IdReservation { get; set; }
        public int? IdProduto { get; set; }
        public int? IdObra { get; set; }
        public int? intClasse { get; set; }
        public string? Classe { get; set; }
        public string? Type { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public int? QtyFinal { get; set; }
        public string? DateReturn { get; set; }
        public DateTime? DateReturnProper { get; set; }
        public string? Observacao { get; set; }
        public int? MemberCodPessoa { get; set; }
        public int? LeaderCodPessoa { get; set; }
        public employeeNewInformationModel? MemberInfo { get; set; } = new employeeNewInformationModel();
        public employeeNewInformationModel? LeaderInfo { get; set; } = new employeeNewInformationModel();


    }

    public class ConsultationModel
    {
        public int? ControlId { get; set; }
        public int? ControlStatus { get; set; }
        public int? LeadercodPessoa { get; set; }
        public string? LeaderName { get; set; }
        public string? ControlType { get; set; }
        public string? ControlStatusString { get; set; }
        public string? DateRegistration { get; set; }
        public string? DateExpiration { get; set; }
        public List<ConsultationReserveModel>? ReservationList { get; set; } = new List<ConsultationReserveModel>();
    }

    public class ConsultationReserveModel
    {
        public string? Classe { get; set; }
        public string? Codigo { get; set; }
        public string? itemNome { get; set; }
        public string? Ferramentaria { get; set; }
        public string? Requester { get; set; }
        public int? Quantidade { get; set; }
        public string? StatusString { get; set; }
        public int? MemberCodPessoa { get; set; }
        public int? LeaderCodPessoa { get; set; }
        public employeeNewInformationModel? MemberInfo { get; set; } = new employeeNewInformationModel();
        public employeeNewInformationModel? LeaderInfo { get; set; } = new employeeNewInformationModel();
        public int? OrderNo { get; set; }
        public string? ReservationType { get; set; }
        public int? IdReservation { get; set; }
        public DateTime? DateReservation { get; set; }
        public string? DateReservationString { get; set; }
    }

    public class GestorRRFilterModel
    {
        public string? Chapa { get; set; }
        public string? Codigo { get; set; }
        public string? Item { get; set; }
        public int? IdCatalogo { get; set; }
        public int? IdClasse { get; set; }
        public int? IdTipo { get; set; }
        public bool? IsChecked { get; set; }
    }

    public class CatalogDetail
    {
        public int? Id { get; set; }
        public int? IdCategoria { get; set; }
        public string? Classe { get; set; }
        public string? ClassType { get; set; }
        public string? Tipo { get; set; }
        public string? Codigo { get; set; }
        public string? Nome { get; set; }
        public string? PorType { get; set; }
        public int? Quantity { get; set; }
        public int? ReservedQuantity { get; set; }
        public int? OverallQuantity { get; set; }
        public int? allocatedQuantity { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? Ferramentaria { get; set; }
        public List<FerramentariaStockModel>? Ferramentarias { get; set; } = new List<FerramentariaStockModel>();
        public bool? Favorited { get; set; }
    }

    public class FerramentariaStockModel
    {
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public int? Quantity { get; set; }
        public int? ReservedQuantity { get; set; }
        public int? ferramentariaAllocatedQuantity { get; set; }
        public int? AvailableQuantity { get; set; }
    }

    public class NewReservationModel
    {
        public int? Id { get; set; }
        public int? IdFerramentaria { get; set; }
        public string? FerramentariaName { get; set; }
        public string? Observacao { get; set; }
    }

}
